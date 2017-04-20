﻿using CodeRefactor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginCore;
using LintingHelper.Managers;
using PluginCore.Localization;

namespace LintingHelper.BatchProcess
{
    class LintProcessor : IBatchProcessor
    {
        public bool IsAvailable
        {
            get
            {
                return LintingManager.HasLanguage(PluginBase.CurrentProject.Language);
            }
        }

        public string Text
        {
            get
            {
                return TextHelper.GetString("Label.RunLinters");
            }
        }

        public LintProcessor()
        {
        }

        public void Process(ITabbedDocument document)
        {
            //Linting is triggered on FileOpen. Since the batch processing dialog opens the files anyways,
            //no linting is required here
            //LintingManager.LintDocument(document);
        }
    }
}
