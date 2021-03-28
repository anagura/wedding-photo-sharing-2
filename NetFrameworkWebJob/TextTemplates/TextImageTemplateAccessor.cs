using functions.Extension;
using System.Collections.Generic;
using System.Linq;

namespace functions.TextTemplates
{
    /// <summary>
    /// テキスト画像テンプレートアクセスクラス
    /// </summary>
    public class TextImageTemplateAccessor
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        static TextImageTemplateAccessor()
        {
            AddList(8, "12071.png", "Big8.xaml");
            AddList(20, "51881.png", "51881.xaml");
            AddList(30, "51893.png", "Middle10.xaml");
            AddList(30, "29831.png", "29831.xaml");
            AddList(40, "82096.png", "TextPanel.xaml");
        }

        /// <summary>
        /// テキスト画像テンプレートリストに追加
        /// </summary>
        /// <param name="limitSize"></param>
        /// <param name="imageName"></param>
        /// <param name="templateName"></param>
        private static void AddList(int limitSize, string imageName, string templateName)
        {
            List.Add(new TextImageTemplate(limitSize, imageName, templateName));
        }

        /// <summary>
        /// テキスト画像テンプレートリスト
        /// </summary>
        protected static List<TextImageTemplate> List { get; } = new List<TextImageTemplate>();

        /// <summary>
        /// シャッフルして1つを抽出
        /// </summary>
        /// <param name="textLength"></param>
        /// <returns></returns>
        public static TextImageTemplate PickRamdom(int textLength)
        {
            return List.Where(x => x.LimitSize >= textLength).PickRandom();
        }
    }
}
