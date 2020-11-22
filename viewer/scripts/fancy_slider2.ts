interface Window {
    initialize_slider: any
}

/* Slider initialization
options:
autoSliding - boolean
autoSlidingDelay - delay in ms. If audoSliding is on and no value provided, default value is 5000
blockASafterClick - boolean. If user clicked any sliding control, autosliding won't start again
*/
window.initialize_slider = (isInitialize: boolean) => {
 //   fncSlider(".example-slider", { autoSlidingDelay: 4000, isInitialize: isInitialize });
    console.log('initialize_slider');
}

