interface Window {
    initialize_slider: any
}
/* Slider initialization
options:
autoSliding - boolean
autoSlidingDelay - delay in ms. If audoSliding is on and no value provided, default value is 5000
blockASafterClick - boolean. If user clicked any sliding control, autosliding won't start again
*/
type options = {
    autoSlidingDelay: number,
    isInitialize: boolean,
}

class fancySlider {

    private static _instance: fancySlider;

    private static getInstance(): fancySlider
    {
        if (!this._instance)
            this._instance = new fancySlider();

        return this._instance;
    }

    public static initialize = (sliderSelector: string, options: options) => {
        const sliders: HTMLCollectionOf<Element> = document.getElementsByClassName(sliderSelector);

        Array(sliders).forEach((slider: HTMLCollection) => {
            fancySlider.getInstance().init(slider, options);
        });
    }

    private init = (slider: HTMLCollection, options: options) => {
        console.log(slider);
        console.log(options);
    }
}

window.initialize_slider = (isInitialize: boolean) => {
    console.log('initialize_slider');
    fancySlider.initialize("example-slider", { autoSlidingDelay: 4000, isInitialize: isInitialize });
}

