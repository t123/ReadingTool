//Displays the rollovers for the help
$(function () {
    $('a.help').hover(
        function () {
            clearTimeout($(this).data('timeout'));
            $(this).parents('form').children('div.input_wrapper').find("div.formhelp").hide();
            $(this).parent().find("div.formhelp").show();
        },
        function () {
            var self = $(this);
            var t = setTimeout(function () {
                self.parent().find("div.formhelp").fadeOut('slow');
            }, 500);
            self.data('timeout', t);
        }
    );
});