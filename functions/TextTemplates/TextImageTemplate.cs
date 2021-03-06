﻿using RazorEngine;
using RazorEngine.Templating;
using System.IO;
using static NetStandardLibraries.Configration.EnvironmentVariables;

namespace functions.TextTemplates
{
    /// <summary>
    /// テキスト画像テンプレート
    /// </summary>
    public class TextImageTemplate
    {
        private const string TemplateDirectoryName = "TextTemplates";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="limitSize"></param>
        /// <param name="imageName"></param>
        /// <param name="templateName"></param>
        public TextImageTemplate(int limitSize, string imageName, string templateName)
        {
            LimitSize = limitSize;
            ImageName = imageName;

            var baseDir = TextImageTemplatePath
                ?? Path.GetFullPath("." + Path.DirectorySeparatorChar);
            var currentDir = Path.Combine(baseDir, TemplateDirectoryName);
            ImagePath = Path.Combine(currentDir, ImageName);
            TemplateName = templateName;
            var templatePath = Path.Combine(currentDir, TemplateName);
            Template = File.ReadAllText(templatePath);
        }

        /// <summary>
        /// パースして文字列に変換
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="textMessage"></param>
        /// <returns></returns>
        public string Parse(string modelName, string textMessage)
        {
            var model = new
            {
                Name = modelName,
                Text = textMessage,
                Source = ImagePath,
            };
            return Engine.Razor
                .RunCompile(Template, ImageName, null, model);
        }

        /// <summary>
        /// 文字数制限
        /// </summary>
        public int LimitSize { get; }

        /// <summary>
        /// イメージ名
        /// </summary>
        public string ImageName { get; }

        /// <summary>
        /// イメージ(フルパス)
        /// </summary>
        public string ImagePath { get; }

        /// <summary>
        /// テンプレート名
        /// </summary>
        public string TemplateName { get; }

        /// <summary>
        /// テンプレート(フルパス)
        /// </summary>
        public string Template { get; }
    }
}
