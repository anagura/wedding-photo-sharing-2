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
    autoSliding?: boolean,
    blockASafterClick?: boolean
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
        var sliders = fancySlider.getInstance().$$(sliderSelector);

        Array(sliders).forEach((slider: HTMLCollection) => {
            fancySlider.getInstance().init(slider, options);
        });
    }

    private options:options;
    private numOfSlides:number = 0;
    private nextSlideId:number = 0;
    static readonly prefix:string = '.fnc-';
    private autoSlidingTO;
    private autoSlidingActive = false;
    private autoSlidingBlocked = false;
    private curSlide:number = 1;
    private sliding = false;
    private $activeSlide;
    private $activeControlsBg;
    private $prevControl;

    private $$ = (selector:string, context:any = null) => {
        var context = context || document;
        let elements = document.querySelectorAll(selector);
        return [].slice.call(elements);
    };

    private setIDs = (slides:any, controls:any, controlsBgs:any ) => {
        slides.forEach(function ($slide, index) {
            const key = "fnc-slide-" + (index + 1);
            if (!$slide.classList.contains(key)) {
                $slide.classList.add(key);
                if (!this.options.isInitialize && this.nextSlideId == 0) {
                    // 追加されたスライドを次に表示する
                    this.nextSlideId = index + 1;
                }
            }
        });

        controls.forEach(function ($control, index) {
            $control.setAttribute("data-slide", index + 1);
            const key = "fnc-nav__control-" + (index + 1);
            if (!$control.classList.contains(key)) {
                $control.classList.add(key);
            }
        });

        controlsBgs.forEach(function ($bg, index) {
            const key = "fnc-nav__bg-" + (index + 1);
            if (!$bg.classList.contains(key)) {
                $bg.classList.add(key);
            }
        });
    };

    
    private init = (slider: any, options: options) => {
        this.options = options;
        let $slider = slider[0];
        const $slidesCont = document.querySelector(fancySlider.prefix + "slider__slides");
        const $slides = this.$$(fancySlider.prefix + "slide", $slider);
        const $controls = this.$$(fancySlider.prefix + "nav__control", $slider);
        const $controlsBgs = this.$$(fancySlider.prefix + "nav__bg", $slider);
        const $progressAS = this.$$(fancySlider.prefix + "nav__control-progress", $slider);
        
        this.numOfSlides = $slides.length;
        var slidingAT = +parseFloat(getComputedStyle($slidesCont)["transition-duration"]) * 1000;
        var slidingDelay = +parseFloat(getComputedStyle($slidesCont)["transition-delay"]) * 1000;

        var autoSlidingDelay = 5000; // default autosliding delay value

        const setIDs = () => {
            $slides.forEach(function ($slide, index) {
                const key = "fnc-slide-" + (index + 1);
                if (!$slide.classList.contains(key)) {
                    $slide.classList.add(key);
                    if (!options.isInitialize && this.nextSlideId == 0) {
                        // 追加されたスライドを次に表示する
                        this.nextSlideId = index + 1;
                    }
                }
            });

            $controls.forEach(function ($control, index) {
                $control.setAttribute("data-slide", index + 1);
                const key = "fnc-nav__control-" + (index + 1);
                if (!$control.classList.contains(key)) {
                    $control.classList.add(key);
                }
            });

            $controlsBgs.forEach(function ($bg, index) {
                const key = "fnc-nav__bg-" + (index + 1);
                if (!$bg.classList.contains(key)) {
                    $bg.classList.add(key);
                }
            });
        };

        setIDs();

        // スライドアニメーション後の待機時間
        const afterSlidingHandler = () => {
            $slider.querySelector(".m--previous-slide").classList.remove("m--active-slide", "m--previous-slide");
            $slider.querySelector(".m--previous-nav-bg").classList.remove("m--active-nav-bg", "m--previous-nav-bg");

            this.$activeSlide.classList.remove("m--before-sliding");
            this.$activeControlsBg.classList.remove("m--nav-bg-before");
            this.$prevControl.classList.remove("m--prev-control");
            this.$prevControl.classList.add("m--reset-progress");
            var triggerLayout = this.$prevControl.offsetTop;
            this.$prevControl.classList.remove("m--reset-progress");

            this.sliding = false;
            var layoutTrigger = $slider.offsetTop;

            if (this.autoSlidingActive && !this.autoSlidingBlocked) {
                setAutoslidingTO();
            }
        };

        // スライドアニメーションの表示
        const performSliding = (slideID) => {
            if (this.sliding) return;
            this.sliding = true;
            window.clearTimeout(this.autoSlidingTO);

            // 表示予約があったらそれを表示
            if (this.nextSlideId > 0) {
                slideID = this.nextSlideId;
                this.nextSlideId = 0;
            }
            this.curSlide = slideID;

            this.$prevControl = $slider.querySelector(".m--active-control");
            this.$prevControl.classList.remove("m--active-control");
            this.$prevControl.classList.add("m--prev-control");
            
            $slider.querySelector(fancySlider.prefix + "nav__control-" + slideID).classList.add("m--active-control");

            this.$activeSlide = $slider.querySelector(fancySlider.prefix + "slide-" + slideID);
            this.$activeControlsBg = $slider.querySelector(fancySlider.prefix + "nav__bg-" + slideID);

            $slider.querySelector(".m--active-slide").classList.add("m--previous-slide");
            $slider.querySelector(".m--active-nav-bg").classList.add("m--previous-nav-bg");

            this.$activeSlide.classList.add("m--before-sliding");
            this.$activeControlsBg.classList.add("m--nav-bg-before");

            var layoutTrigger = this.$activeSlide.offsetTop; // 謎の処理だが、これがないとアニメーションしない

            this.$activeSlide.classList.add("m--active-slide");
            this.$activeControlsBg.classList.add("m--active-nav-bg");

            // アニメーションが完了したら
            setTimeout(afterSlidingHandler, slidingAT + slidingDelay);
        };

        function controlClickHandler() {
            // 自分自身を表示中だったら処理しない
            if (this.sliding) return;

            if (this.classList.contains("m--active-control")) return;

            // 表示予約があったらキャンセルする
            if (this.nextSlideId > 0) {
                this.nextSlideId = 0;
            }
            if (options?.blockASafterClick) {
                this.autoSlidingBlocked = true;
                $slider.classList.add("m--autosliding-blocked");
            }

            var slideID = +this.getAttribute("data-slide");

            performSliding(slideID);
        };

        $controls.forEach(function ($control, index) {
            const key = "fnc-listener-add-" + (index + 1);
            if (!$control.classList.contains(key)) {
                $control.classList.add(key);
                $control.addEventListener("click", controlClickHandler);
            }
        });

        const setAutoslidingTO = () => {
            window.clearTimeout(this.autoSlidingTO);
            var delay = +options.autoSlidingDelay || autoSlidingDelay;
            this.curSlide++;
            if (this.curSlide > this.numOfSlides) this.curSlide = 1;

            this.autoSlidingTO = setTimeout(() => {
                performSliding(this.curSlide);
            }, delay);
        };

        if (options.isInitialize) {
            if (options?.autoSliding || +options.autoSlidingDelay > 0) {
                if (options?.autoSliding === false) return;

                this.autoSlidingActive = true;

                setAutoslidingTO();

                if (!$slider.classList.contains("m--with-autosliding")) {
                    $slider.classList.add("m--with-autosliding");
                }
                var triggerLayout = $slider.offsetTop;

                var delay = +options.autoSlidingDelay || autoSlidingDelay;
                delay += slidingDelay + slidingAT;

                $progressAS.forEach(function ($progress) {
                    $progress.style.transition = "transform " + (delay / 1000) + "s";
                });
            }

            $slider.querySelector(".fnc-nav__control:first-child").classList.add("m--active-control");
        } else {
            var delay = +options.autoSlidingDelay || autoSlidingDelay;
            delay += slidingDelay + slidingAT;

            $progressAS.forEach(function ($progress) {
                $progress.style.transition = "transform " + (delay / 1000) + "s";
            });
        }

    }
}

window.initialize_slider = (isInitialize: boolean) => {
    fancySlider.initialize(".example-slider", { autoSlidingDelay: 4000, isInitialize: isInitialize });
}

