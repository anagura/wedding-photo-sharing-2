using functions.TextTemplates;
using ImageGeneration;
using Microsoft.Azure.WebJobs;
using NetStandardLibraries.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetFrameworkWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static Task ProcessQueueMessage([QueueTrigger("messages")] string message, TextWriter log)
        {
            log.WriteLine($"new message is {message}");

            var entity = JsonSerializer.Deserialize<MessageEntity>(message);
            log.WriteLine($"new message is {entity.Name}, {entity.Message}");
            Console.WriteLine($"new message is {entity.Name}, {entity.Message}");

            // テンプレートよりランダム抽出
            var template = TextImageTemplateAccessor.PickRamdom(entity.Message.Length);

            // テンプレートよりバイト配列生成
            var inputXaml = template.Parse(entity.Name, entity.Message);
//            var image = ImageConverter.ConvertFromXaml(inputXaml);
            dynamic viewModel = new ExpandoObject();
            viewModel.Name = entity.Name;
            viewModel.Text = entity.Message;
            var image = ImageGenerator.GenerateImage(inputXaml, viewModel);

            FileStream fs = new FileStream("test.png", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(image);
            bw.Close();
            fs.Close();

            return Task.CompletedTask;
        }
    }
}
