using System.IO;

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
            TemplateName = templateName;
            var currentDir = Path.Combine(Directory.GetCurrentDirectory(), TemplateDirectoryName);
            var templatePath = Path.Combine(currentDir, TemplateName);
            Template = File.ReadAllText(templatePath);
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
        /// テンプレート名
        /// </summary>
        public string TemplateName { get; }

        /// <summary>
        /// テンプレート(フルパス)
        /// </summary>
        public string Template { get; }
    }
}
