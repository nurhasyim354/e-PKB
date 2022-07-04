(function($) {
    $.fn.skim = function(valHeight, valWidth) {
        return this.each(function() {
            var container = $(this);
            var imgHeight = valHeight;
            var imgWidth = valWidth;
            var numImgs = container.find("li").size();
            var zoneWidth = imgWidth / numImgs;
            ////container.css("overflow", "hidden");
            ////container.css("height", imgHeight);
            ////container.css("width", imgWidth);
            //container.find("ul").css("list-style", "none");
            //container.find("ul").css("padding", 0);
            ////container.find("ul").css("float", "left");
            ////container.find("ul").css("width", imgWidth);
            ////container.find("ul").css("height", imgHeight);
            container.find("ul").css("position", "relative");
            if (numImgs > 1) {
                ////container.find("ul li").css("float", "left");
                container.find("li").css("display", "none");
                container.find("li").css("width", "100%");
                container.find("li").css("top", "0");
                container.find("li").css("position", "absolute");
                container.find("li:first").css("position", "relative");
                container.find("li:first").css("display", "block");
                ////container.find("ul li:first").css("float", "left");
                if (window.innerWidth > 990) {
                    container.find("ul").mousemove(function (e) {
                        var offset = container.offset();
                        x = e.pageX - offset.left;
                        var currentZone = Math.floor(x / zoneWidth);
                        if (currentZone >= numImgs) currentZone = numImgs - 1;
                        $(this).find("li").css("display", "none");
                        $(this).find("li:first").css("display", "block");
                        $(this).find("li:eq(" + currentZone + ")").css("display", "block");
                    });
                    container.find("ul").mouseleave(function (e) {
                        $(this).find("li").css("display", "none");
                        $(this).find("li:first").css("display", "block");
                        $(this).find("li:eq(0)").css("display", "block");
                    });
                }
            }
        })
    }
})(jQuery);