dewd = {};

dewd.scrollgrid = function ($container, $autoHeightElement, autoHeightOffset) {
    // make fixed header on grid
    var $j = jQuery;
    var $header = $j("table.grid tr:eq(0)", $container);

    var $headerTableCols, fixedHeaderHandler = function () { };
    if ($header.size() == 0) {
        // no rows, remove space for fixed header
        autoHeightOffset -= 26;
    } else {
        var $headerCols = $j("th", $header);
        var table = "<table style='border-collapse:collapse;table-layout:fixed;' cellspacing='0' border='0'><tr>" + $header.clone().html() + "</tr></table>";
        $headerTableCols = $j("table:first th", $container.prepend(table));

        $j(".scroll", $container).focus();
        $j(".grid tr", $container).hover(
                    function () { $j(this).addClass("hover"); },
                    function () { $j(this).removeClass("hover"); }
                );

        fixedHeaderHandler = function () {
            var $secondRow = $j("table.grid tr:eq(1) td", $container);
            $secondRow.each(function (i) {
                $this = $j(this);
                var width = $this.width() + 1;
                $j($headerTableCols.get(i)).css("width", width);
                var newWidth = $j($headerTableCols.get(i)).width();
                if (newWidth > width)
                    $this.css("width", newWidth); // forces data col to header width
            });

            //$j("#container table.grid").css("table-layout", "fixed");
            $header.hide();
        };
    }

    var resizeHandler = function () {
        var height = $autoHeightElement.innerHeight() - autoHeightOffset;
        $j(".scroll", $container).css("height", height);
        fixedHeaderHandler();
    };

    // not pretty but we want to make sure things are done asap and this works: 
    setTimeout(function () { resizeHandler(); resizeHandler(); }, 50);
    setInterval(resizeHandler, 1000);
};
