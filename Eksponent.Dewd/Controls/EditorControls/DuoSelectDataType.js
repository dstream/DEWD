jQuery(function () {
    $("table.duoSelect").each(function () {
        var $container = $(this);

        var $left = $container.find("select:eq(0)");
        var $right = $container.find("select:eq(1)");
        //$right.css("height", $left.outerHeight() + "px");
        var $valueField = $container.find("input[type=hidden]");

        $left.data("left", 1);

        function sort(event, direction) {
            event.stopPropagation();
            event.preventDefault();

            var $option = $right.children("option:selected");
            if (direction == 0)
                $option.prev().before($option);
            else
                $option.next().after($option);
            updateValue();
        }

        function updateValue() {
            var val = $right.children("option").map(function () { return this.value; }).get().join(',');
            $valueField.attr("value", val);
        }

        function moveOption($source, $target, index, $option) {
            if (index != null)
                $option = $("option:eq(" + index + ")", $source);

            var $newOption = $(document.createElement("option"))
                .attr("value", $option.attr("value"))
                .text($option.text());
            $target.append($newOption);
            $option.remove();
        }

        function deselect() {
            $left.children("option").attr("selected", "");
            $right.children("option").attr("selected", "");
        }

        function move(event) {
            if (this.selectedIndex == -1)
                return;
            var $source = ($(this).data("left") == 1 ? $left : $right);
            var $target = ($(this).data("left") == 1 ? $right : $left);

            //alert(this.selectedIndex);
            //alert("mving: " + this.selectedIndex + ", source==left: " + ($source.data("left") == 1) + ", target==left: " + ($target.data("left") == 1));
            moveOption($source, $target, this.selectedIndex);
            updateValue();
        }

        // move single events
        $left.dblclick(move);
        $right.dblclick(move);

        // sort events
        $container.find(".sort span").each(function (i) {
            $(this)
                .css("cursor", "pointer")
                .click(function (event) { sort(event, i); })
                .dblclick(function (event) { event.stopPropagation(); event.preventDefault(); })
                .attr("unselectable", "on");
        });

        // move multiple events
        $container.find(".move span:eq(0)").click(function () {
            $right.children("option").each(function (i) {
                if ($(this).attr("selected")) moveOption($right, $left, null, $(this));
            });
            updateValue();
        }).css("cursor", "pointer");
        $container.find(".move span:eq(1)").click(function () {
            $left.children("option").each(function (i) {
                if ($(this).attr("selected")) moveOption($left, $right, null, $(this));
            });
            updateValue();
        }).css("cursor", "pointer");

        // set initial value
        $.each($valueField.attr("value").split(','), function () {
            if (this.length != 0) {
                var $selectedOption = $left.children("option[value='" + this + "']");
                if ($selectedOption.length != 0)
                    moveOption($left, $right, null, $selectedOption);
                else
                    alert("DuoSelectDataType.js: selected option not found in source: " + this);
            }
        });
        //deselect();
    });
});