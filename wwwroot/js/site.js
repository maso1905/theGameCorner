
// Active nav-link

$(function () {
    var current = location.pathname;
    $(".nav-tabs li a").removeClass("active");
    $('.nav-tabs li a').each(function () {
        var $this = $(this);
        if (current.indexOf($this.attr('href')) !== -1) {
            $this.addClass('active');
        }
    })
})

// Enable menu dropdown toggle
$('.dropdown-toggle').dropdown()

// Prevent form from submitting if an image is not selected.
$('#upload').bind("click", function () {

    var imgVal = $('#uploadImage').val();
    if (imgVal == '') {
        alert("Please select an image");
        $('form').submit(function (e) {
            e.preventDefault();
        });
        location.reload();
    }    
}); 