/*=======================================
            Navigation Bar
========================================*/

/* Show & Hide White Navigation */
$(function () {

    // show/hide nav on page load
    showHideNav();

    $(window).scroll(function () {

        // show/hide nav on window's scroll
        showHideNav();

    });

    function showHideNav() {

        if ($(window).scrollTop() > 50) {

            //show white nav
            $("nav").addClass("white-nav-top");

            // show dark logo
            $(".navbar-brand img").attr("src", "../images/home/logo.png");

            // show back to top button
            $("#back-to-top").fadeIn();

        } else {

            //Hide white nav
            $("nav").removeClass("white-nav-top");

            // show logo
            $(".navbar-brand img").attr("src", "../images/home/top-logo.png");

            // Hide back to top button
            $("#back-to-top").fadeOut();
        }

    }

});



$(function () {

    $("a.smooth-scroll").click(function (event) {

        event.preventDefault();

        //get section id like #about, #services, #work and etc.
        var section_id = $(this).attr("href");

        $("html, body").animate({
            scrollTop: $(section_id).offset()
        }, 1250, "easeInOutExpo");

    });

});


/*=======================================
            Login
========================================*/

$(".toggle-password").click(function () {

    var input = $($(this).attr("toggle"));
    if (input.attr("type") == "password") {
        input.attr("type", "text");
    } else {
        input.attr("type", "password");
    }
});


/*=======================================
        Selected Value colour change
========================================*/
$(document).ready(function () {
    $('select').css('color', '#8a8a8a');
    $('select').change(function () {
        var current = $('select').val();
        if (current != null) {
            $('select').css('color', '#333');
        } else {
            $('select').css('color', '#d1d1d1')
        }
    });
});



/*=======================================
            Mobile Menu
========================================*/

$(function () {

    if ($(window).width < 426) {
        $('nav').addClass('display-btn');
    }

    //show mobile nav
    $("#mobile-nav-open-btn").click(function () {
        $("#mobile-nav").css("height", "100%");
    });
    $("#mobile-nav-close-btn, #mobile-nav a").click(function () {
        $("#mobile-nav").css("height", "0%");
    });
});