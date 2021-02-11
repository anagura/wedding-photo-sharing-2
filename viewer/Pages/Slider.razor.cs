﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using viewer.Components;

namespace viewer.Pages
{
    public partial class Slider
    {
    private bool initializeJs = true;
    private bool initialized;
    private Dictionary<long, MessageEntity> messages = new Dictionary<long, MessageEntity>();

    private bool inTimer = false;
    private readonly int delay = 10000;

    private async Task AddSlide()
    {
        var tmp_messages = await Http.GetFromJsonAsync<MessageEntity[]>("https://satoweddingphotosharing.azurewebsites.net/api/FetchImage");
        foreach (var msg in tmp_messages)
        {
            if (!messages.ContainsKey(msg.Id))
            {
                messages.Add(msg.Id, msg);
            }
        }
        initializeJs = false;
        initialized = false;
    }

    private string bgBlendType(Slide.BlendType blendType)
    {
        string result = "";
        switch (blendType)
        {
            case Slide.BlendType.Green:
                result = "m--navbg-green";
                break;
            case Slide.BlendType.Dark:
                result = "m--navbg-dark";
                break;
            case Slide.BlendType.Red:
                result = "m--navbg-red";
                break;
            case Slide.BlendType.Blue:
                result = "m--navbg-blue";
                break;
        }
        return result;
    }
    private string bgIsActive(bool isActive)
    {
        return isActive ? "m--active-nav-bg" : "";
    }

    private async Task HandleTimerCallback(object state)
    {
        if (inTimer)
        {
            return;
        }
        inTimer = true;
        await AddSlide();
        this.StateHasChanged();
        inTimer = false;
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var tmp_messages = await Http.GetFromJsonAsync<MessageEntity[]>("https://satoweddingphotosharing.azurewebsites.net/api/FetchImage");

            int i = 0;
            foreach (var msg in tmp_messages)
            {
                msg.IsActive = i == 0;  // TODO:
                messages.Add(msg.Id, msg);
                i++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        var timer = new Timer(async o => await HandleTimerCallback(o), null, delay, delay);
    }

    public async Task TriggerJsPrompt(bool isInitialize)
    {
        await JSRuntime.InvokeVoidAsync(
            "initialize_slider", new object[] { isInitialize });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender && messages != null && !initialized)
        {
            await TriggerJsPrompt(initializeJs);
            initialized = true;
        }
    }

    public class MessageEntity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Message { get; set; }

        public string ImageUrl { get; set; }

        public string ThunbnailImageUrl { get; set; }

        public string RawImageUrl { get; set; }

        public bool IsActive { get; set; }
        public string BlendTypeString { get; set; }
        public Slide.BlendType BlendType
        {
            get
            {
                Slide.BlendType result = Slide.BlendType.Green;
                switch (BlendTypeString)
                {
                    case "Green":
                        result = Slide.BlendType.Green;
                        break;
                    case "Dark":
                        result = Slide.BlendType.Dark;
                        break;
                    case "Red":
                        result = Slide.BlendType.Red;
                        break;
                    case "Blue":
                        result = Slide.BlendType.Blue;
                        break;
                }
                return result;
            }
        }
    }

    }
}
