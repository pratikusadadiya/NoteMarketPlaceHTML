/*===============================
		 Hide/Show password
=================================*/
$(".toggle-password").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});

/*========================
        Mobile Menu
=========================*/
$(function () {

    // show mobile nav
    $("#mobile-nav-open-btn").click(function () {
        $("#mobile-nav").css("height", "100%");
    });

    // hide mobile nav
    $("#mobile-nav-close-btn, #mobile-nav a").click(function () {
        $("#mobile-nav").css("height", "0%");
    });

});

/*========================
        Navigation
=========================*/

/* Show and hide white Navigation */
$(function () {

    // show/hide Navigation on page load
    showHideNav();

    $(window).scroll(function () {

        // show/hide navigation on window scroll
        showHideNav();

    });

    function showHideNav() {
        if ($(window).scrollTop() > 50) {
            
            // Show White Nav & blue logo
            $("nav").addClass("white-nav-top");
            $("#mobile-nav-open-btn").css("color", "#6255a5");
            
            $(".navbar-brand img").attr("src", "../../Content/images/Front/assets/logo-img.png");
            

        } else {
            // Hide White Nav & Show white logo
            $("nav").removeClass("white-nav-top");
            $(".navbar-brand img").attr("src", "../../Content/images/Front/assets/top-logo.png");
            $("#mobile-nav-open-btn").css("color", "#fff");
            
            $("#top-fixed-nav #mobile-nav-open-btn").css("color", "#6255a5");
            $("#top-fixed-nav .navbar-brand img").attr("src", "../../Content/images/Front/assets/logo-img.png");
        }

    }

});

/*====================================
        Search Notes filters
======================================*/
jQuery(document).ready(function($) {
  var alterClass = function() {
    var ww = document.body.clientWidth;
    if (ww <= 480) {
        
        $('#search-notes .col-xs-6').removeClass('col-xs-6');
        $('#search-notes .col-xs-6').addClass('col-xs-12');
        
        $('.table-heading .col-md-6').removeClass('col-xs-6');
        $('.table-heading .col-md-6').addClass('col-xs-12');
        
    } else if (ww > 480) {
        
        $('#search-notes .col-xs-6').removeClass('col-xs-12');
        $('#search-notes .col-xs-6').addClass('col-xs-6');
        
        $('.table-heading .col-md-6').removeClass('col-xs-12');
        $('.table-heading .col-md-6').addClass('col-xs-6');
        
    };
    
    /* Table responsive class add & Remove */
    if (ww <= 767) {
        
        $('.table-section #table-content').addClass('table-responsive');
        
        
    } else if (ww > 768) {
        
        $('.table-section #table-content').removeClass('table-responsive');
       
        
    };
  };
  $(window).resize(function(){
    alterClass();
  });
  //Fire it when the page first loads:
  alterClass();
});

/*========================================
        FAQ question answer
==========================================*/
$(document).ready(function () {

    for (let i = 1; i <= 7; i++) {
        $(".expand" + i).click(function () {
            $(".mybody" + i).show();
            $(".myhead" + i).hide();
        });
        $(".collapse" + i).click(function () {
            $(".mybody" + i).hide();
            $(".myhead" + i).show();
        });
    }
});

























