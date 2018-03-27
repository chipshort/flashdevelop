﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;

namespace ASCompletion.Helpers
{
    using CacheDictionary = Dictionary<MemberModel, HashSet<ClassModel>>;

    delegate void CacheUpdated();

    internal class ASTCache
    {
        public event Action FinishedUpdate;

        Dictionary<ClassModel, CachedClassModel> cache =
            new Dictionary<ClassModel, CachedClassModel>(CacheHelper.classModelComparer);

        readonly HashSet<ClassModel> outdatedModels = new HashSet<ClassModel>(CacheHelper.classModelComparer);
        /// <summary>
        /// A list of ClassModels that extend / implement something that does not exist yet
        /// </summary>
        readonly HashSet<ClassModel> unfinishedModels = new HashSet<ClassModel>(CacheHelper.classModelComparer);

        private int outdateUpdateCount;

        public CachedClassModel GetCachedModel(ClassModel cls)
        {
            CachedClassModel v;
            cache.TryGetValue(cls, out v);
            return v;
        }

        public void Clear()
        {
            lock (outdatedModels)
                outdatedModels.Clear();
            lock (unfinishedModels)
                unfinishedModels.Clear();
            cache.Clear();
        }

        public void Remove(ClassModel cls)
        {
            lock (cache)
            {
                var cachedClassModel = GetCachedModel(cls);

                if (cachedClassModel == null) return;

                var implementing = cachedClassModel.Implementing;
                var overriding = cachedClassModel.Overriding;
                var implementors = cachedClassModel.Implementors;
                var overriders = cachedClassModel.Overriders;

                //remove old references to cls
                RemoveConnections(cls, implementing, ccm => ccm.Implementors);
                RemoveConnections(cls, overriding, ccm => ccm.Overriders);
                RemoveConnections(cls, implementors, ccm => ccm.Implementing);
                RemoveConnections(cls, overriders, ccm => ccm.Overriding);

                //remove connected classes hashset
                foreach (var clsModel in cachedClassModel.ConnectedClassModels)
                {
                    CachedClassModel cachedClass;
                    if (cache.TryGetValue(clsModel, out cachedClass))
                        cachedClass.ConnectedClassModels.Remove(clsModel);
                }

                //remove cls itself
                cache.Remove(cls);
            }
        }

        public void MarkAsOutdated(ClassModel cls)
        {
            lock (outdatedModels)
                outdatedModels.Add(cls);
        }

        public void UpdateOutdatedModels()
        {
            try
            {
                var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
                if (context?.Classpath == null)
                    return;

                HashSet<ClassModel> outdated;
                lock (outdatedModels)
                {
                    outdated = new HashSet<ClassModel>(outdatedModels, outdatedModels.Comparer);
                    outdatedModels.Clear();
                }

                var newModels = outdated.Any(m => GetCachedModel(m) == null);

                Interlocked.Increment(ref outdateUpdateCount);

                foreach (var cls in outdated)
                {
                    cls.ResolveExtends();

                    lock (cache)
                    {
                        //get the old CachedClassModel
                        var cachedClassModel = GetCachedModel(cls);
                        var connectedClasses = cachedClassModel?.ConnectedClassModels;

                        //remove old cls
                        Remove(cls);

                        UpdateClass(cls, cache);

                        //also update all classes / interfaces that are connected to cls
                        if (connectedClasses != null)
                            foreach (var connection in connectedClasses)
                                if (GetCachedModel(connection) != null)
                                    //only update existing connections, so a removed class is not reintroduced
                                    UpdateClass(connection, cache);
                    }
                }

                //for new ClassModels, we need to update everything in the list of classes that extend / implement something that does not exist
                if (newModels)
                {
                    HashSet<ClassModel> toUpdate;
                    lock (unfinishedModels)
                        toUpdate = new HashSet<ClassModel>(unfinishedModels, unfinishedModels.Comparer);

                    foreach (var model in toUpdate)
                    {
                        lock (unfinishedModels)
                            unfinishedModels.Remove(model); //will be added back by UpdateClass if needed

                        lock (cache)
                            UpdateClass(model, cache);
                    }
                }

                if (Interlocked.Decrement(ref outdateUpdateCount) == 0 && FinishedUpdate != null)
                    PluginBase.RunAsync(new MethodInvoker(FinishedUpdate));
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Update the cache for the whole project
        /// </summary>
        public void UpdateCompleteCache()
        {
            try
            {
                var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
                if (context?.Classpath == null || PathExplorer.IsWorking)
                {
                    if (FinishedUpdate != null)
                        PluginBase.RunAsync(new MethodInvoker(FinishedUpdate));
                    return;
                }

                var c = new Dictionary<ClassModel, CachedClassModel>(CacheHelper.classModelComparer);

                foreach (MemberModel memberModel in context.GetAllProjectClasses())
                {
                    if (PluginBase.MainForm.ClosingEntirely)
                        return; //make sure we leave if the form is closing, so we do not block it

                    var cls = GetClassModel(memberModel);
                    UpdateClass(cls, c);
                }

                lock (cache)
                    cache = c;

                if (FinishedUpdate != null)
                    PluginBase.RunAsync(new MethodInvoker(FinishedUpdate));
                }
            catch (Exception)
            {
            }
        }

        internal HashSet<ClassModel> ResolveInterfaces(ClassModel cls)
        {
            if (cls == null || cls.IsVoid()) return new HashSet<ClassModel>(CacheHelper.classModelComparer);
            if (cls.Implements == null) return ResolveInterfaces(cls.Extends);

            var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            return cls.Implements
                .Select(impl => context.ResolveType(impl, cls.InFile))
                .Where(interf => interf != null && !interf.IsVoid())
                .SelectMany(interf => //take the interfaces we found already and add all interfaces they extend
                {
                    interf.ResolveExtends();
                    var set = ResolveExtends(interf);
                    set.Add(interf);
                    return set;
                })
                .Union(ResolveInterfaces(cls.Extends)).ToHashSet(CacheHelper.classModelComparer);
        }

        /// <summary>
        /// Checks if the class extends / implements something that does not exist (yet).
        /// </summary>
        /// <returns>True if nothing is found, false if there are non-existing parents</returns>
        /// <param name="cls"></param>
        bool IsCompletelyResolvable(ClassModel cls)
        {
            if (cls == null || cls.IsVoid()) return true;

            var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);

            var missingExtends = cls.ExtendsType != "Dynamic" && cls.ExtendsType != "Void" && cls.ExtendsType != null && cls.Extends.IsVoid(); //Dynamic means the class extends nothing
            var missingInterfaces = cls.Implements != null && cls.Implements.Any(i => GetCachedModel(context.ResolveType(i, cls.InFile)) == null);

            //also check parent interfaces and extends
            return !missingInterfaces && !missingExtends && (cls.Implements == null || ResolveInterfaces(cls).All(IsCompletelyResolvable)) && IsCompletelyResolvable(cls.Extends);
        }

        void RemoveConnections(ClassModel cls, CacheDictionary goThrough, Func<CachedClassModel, CacheDictionary> removeFrom)
        {
            //for easier understandability, the comments below explain the process using the following example:
            //goThrough := GetCachedModel(cls).Implementing
            //removeFrom(c) := c.Implementors
            foreach (var pair in goThrough)
            {
                //go through all interfaces cls implements
                foreach (var interf in pair.Value)
                {
                    var ccm = GetCachedModel(interf);
                    if (ccm == null) continue; //should not happen

                    //remove all occurences of cls from the interface's implementors
                    var toRemove = new HashSet<MemberModel>();
                    var from = removeFrom(ccm);
                    foreach (var p in from)
                    {
                        p.Value.Remove(cls);

                        if (p.Value.Count == 0)
                            toRemove.Add(p.Key); //empty ones should be removed
                    }

                    foreach (var r in toRemove)
                        from.Remove(r);
                }
            }
        }

        /// <summary>
        /// Updates the given ClassModel in cache. This assumes that all existing references to cls in the cache are still correct.
        /// However they do not have to be complete, this function will add missing connections based on cls.
        /// </summary>
        void UpdateClass(ClassModel cls, Dictionary<ClassModel, CachedClassModel> classModelCache)
        {
            var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);

            if (context.ResolveType(cls.Name, cls.InFile).IsVoid() || cls.QualifiedName == "Dynamic") //do not update no longer existing classes (or Dynamic)
            {
                Remove(cls);
                return;
            }
            cls.ResolveExtends();
            var cachedClassModel = GetOrCreate(classModelCache, cls);

            //look for functions / variables in cls that originate from interfaces of cls
            var interfaces = ResolveInterfaces(cls);

            foreach (var interf in interfaces)
            {
                var cachedInterf = GetOrCreate(classModelCache, interf);
                cachedClassModel.ConnectedClassModels.Add(interf); //cachedClassModel is connected to interf.Key
                cachedInterf.ConnectedClassModels.Add(cls); //the inverse is also true

                cachedInterf.ImplementorClassModels.Add(cls); //cls implements interf
            }

            if (interfaces.Count > 0)
            {
                //look at each member and see if one of them is defined in an interface of cls
                foreach (MemberModel member in cls.Members)
                {
                    var implementing = GetDefiningInterfaces(member, interfaces);

                    if (implementing.Count == 0) continue;

                    cachedClassModel.Implementing.AddUnion(member, implementing.Keys, CacheHelper.classModelComparer);
                    //now that we know member is implementing the interfaces in implementing, we can add cls as implementor for them
                    foreach (var interf in implementing)
                    {
                        var cachedModel = GetOrCreate(classModelCache, interf.Key);
                        var set = CacheHelper.GetOrCreateSet(cachedModel.Implementors, interf.Value, CacheHelper.classModelComparer);
                        set.Add(cls);
                    }
                }
            }

            if (cls.Extends != null && !cls.Extends.IsVoid())
            {

                var currentParent = cls.Extends;
                while (currentParent != null && !currentParent.IsVoid())
                {
                    var cachedParent = GetOrCreate(classModelCache, currentParent);
                    cachedClassModel.ConnectedClassModels.Add(currentParent); //cachedClassModel is connected to currentParent
                    cachedParent.ConnectedClassModels.Add(cls); //the inverse is also true

                    cachedParent.ChildClassModels.Add(cls); //cls implements interf
                    currentParent = currentParent.Extends;
                }
                //look for functions in cls that originate from a super-class
                foreach (MemberModel member in cls.Members)
                {
                    if ((member.Flags & (FlagType.Function | FlagType.Override)) > 0)
                    {
                        var overridden = GetOverriddenClasses(cls, member);

                        if (overridden == null || overridden.Count <= 0) continue;

                        cachedClassModel.Overriding.AddUnion(member, overridden.Keys, CacheHelper.classModelComparer);
                        //now that we know member is overriding the classes in overridden, we can add cls as overrider for them
                        foreach (var over in overridden)
                        {
                            var cachedModel = GetOrCreate(classModelCache, over.Key);
                            var set = CacheHelper.GetOrCreateSet(cachedModel.Overriders, over.Value, CacheHelper.classModelComparer);
                            set.Add(cls);
                        }
                    }
                }
            }

            if (!IsCompletelyResolvable(cls))
                lock (unfinishedModels)
                    unfinishedModels.Add(cls);
        }

        /// <summary>
        /// Gets all ClassModels from <paramref name="interfaces"/> and the interfaces they extend that contain a definition of <paramref name="member"/>
        /// </summary>
        /// <returns>A Dictionary containing all pairs of ClassModels and MemberModels were implemented by <paramref name="member"/></returns>
        internal Dictionary<ClassModel, MemberModel> GetDefiningInterfaces(MemberModel member, HashSet<ClassModel> interfaces)
        {
            //look for all ClassModels with variables / functions of the same name as member
            //this could give faulty results if there are variables / functions of the same name with different signature in the interface
            var implementors = new Dictionary<ClassModel, MemberModel>(CacheHelper.classModelComparer);

            foreach (var interf in interfaces)
            {
                var interfMember = interf.Members.Search(member.Name, 0, 0);
                if (interfMember != null)
                    implementors.Add(interf, interfMember);
            }

            return implementors;
        }

        /// <summary>
        /// Gets all ClassModels that are a super-class of the given <paramref name="cls"/> and contain a function that is overridden
        /// by <paramref name="function"/>
        /// </summary>
        /// <returns>A Dictionary containing all pairs of ClassModels and MemberModels that were overridden by <paramref name="function"/></returns>
        internal Dictionary<ClassModel, MemberModel> GetOverriddenClasses(ClassModel cls, MemberModel function)
        {
            if (cls.Extends == null || cls.Extends.IsVoid()) return null;
            if ((function.Flags & FlagType.Function) == 0 || (function.Flags & FlagType.Override) == 0) return null;

            var parentFunctions = new Dictionary<ClassModel, MemberModel>(CacheHelper.classModelComparer);

            var currentParent = cls.Extends;
            while (currentParent != null && !currentParent.IsVoid())
            {
                var parentFun = currentParent.Members.Search(function.Name, FlagType.Function, 0); //overridden function can have different access
                //it should not be necessary to check the parameters, because two functions with different signature cannot have the same name (at least in Haxe)

                if (parentFun != null)
                    parentFunctions.Add(currentParent, parentFun);

                currentParent = currentParent.Extends;
            }

            return parentFunctions;
        }

        /// <summary>
        /// Returns a set of all ClassModels that <paramref name="cls"/> extends
        /// </summary>
        static HashSet<ClassModel> ResolveExtends(ClassModel cls)
        {
            var set = new HashSet<ClassModel>(CacheHelper.classModelComparer);

            var current = cls.Extends;
            while (current != null && !current.IsVoid())
            {
                set.Add(current);
                current = current.Extends;
            }

            return set;
        }

        static CachedClassModel GetOrCreate(Dictionary<ClassModel, CachedClassModel> cache, ClassModel cls)
        {
            CachedClassModel cached;
            if (!cache.TryGetValue(cls, out cached))
            {
                cached = new CachedClassModel();
                cache.Add(cls, cached);
            }

            return cached;
        }

        static ClassModel GetClassModel(MemberModel cls)
        {
            var pos = cls.Type.LastIndexOf('.');
            var package = pos == -1 ? "" : cls.Type.Substring(0, pos);
            var name = cls.Type.Substring(pos + 1);

            var context = ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            return context.GetModel(package, name, "");
        }
    }

    class CachedClassModel
    {
        public HashSet<ClassModel> ConnectedClassModels = new HashSet<ClassModel>(CacheHelper.classModelComparer);

        /// <summary>
        /// A set of ClassModels that extend this ClassModel
        /// </summary>
        public HashSet<ClassModel> ChildClassModels = new HashSet<ClassModel>(CacheHelper.classModelComparer);

        /// <summary>
        /// If this ClassModel is an interface, this contains a set of classes that implement this interface
        /// </summary>
        public HashSet<ClassModel> ImplementorClassModels = new HashSet<ClassModel>(CacheHelper.classModelComparer);


        /// <summary>
        /// If this ClassModel is an interface, this contains a set of classes - for each member - that implement the given members
        /// </summary>
        public CacheDictionary Implementors = new CacheDictionary();

        /// <summary>
        /// Contains a set of interfaces - for each member - that contain the member.
        /// </summary>
        public CacheDictionary Implementing = new CacheDictionary();

        /// <summary>
        /// Contains a set of classes - for each member - that override the member.
        /// </summary>
        public CacheDictionary Overriders = new CacheDictionary();

        /// <summary>
        /// Contains a set of classes - for each member - that this member overrides
        /// </summary>
        public CacheDictionary Overriding = new CacheDictionary();
    }

    static class CacheHelper
    {
        internal static ClassModelComparer classModelComparer = new ClassModelComparer();

        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> e, IEqualityComparer<T> comparer)
        {
            if (e == null) return new HashSet<T>(comparer);

            return new HashSet<T>(e, comparer);
        }

        internal static void AddUnion<S, T>(this Dictionary<S, HashSet<T>> dict, S key, IEnumerable<T> value, IEqualityComparer<T> comparer)
        {
            var set = GetOrCreateSet(dict, key, comparer);

            set.UnionWith(value);
        }

        internal static ISet<T> GetOrCreateSet<S, T>(Dictionary<S, HashSet<T>> dict, S key, IEqualityComparer<T> comparer)
        {
            HashSet<T> set;
            if (!dict.TryGetValue(key, out set))
            {
                set = new HashSet<T>(comparer);
                dict.Add(key, set);
            }
            return set;
        }
    }

    internal class ClassModelComparer : IEqualityComparer<ClassModel>
    {
        public bool Equals(ClassModel x, ClassModel y)
        {
            if (x == null || y == null) return x == y;

            return x.Type == y.Type && x.InFile?.FileName == y.InFile?.FileName;
        }

        public int GetHashCode(ClassModel obj) =>
            !string.IsNullOrEmpty(obj.InFile?.FileName)
                ? obj.InFile.FileName.GetHashCode() ^ obj.BaseType.GetHashCode()
                : obj.BaseType.GetHashCode();
    }
}
