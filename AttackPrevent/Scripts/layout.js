
$(document).ready(function () {
    $(".headerRight").hover(function () {
        console.log("aaaa");
        $(this).children(".divAvatarsHasTop").stop(false, true).slideDown(300);
    }, function () {
        $(this).children(".divAvatarsHasTop").stop(false, true).css("display", "none");
    }
    );
});

