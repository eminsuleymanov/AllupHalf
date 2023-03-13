$(document).ready(function () {
    $(document).on('click', '.deleteImage', function(e)
    {
        e.preventDefault();
        let url = $('.deleteImage').attr('href');
        let imageId = $('.deleteImage').attr('data-imageId');
        fetch(url + "?imageId=" + imageId)
            .then(res => {
                if (res.ok) {
                    return res.text()
                } else {
                    alert("Yanlish emeliyyat")
                    return
                }
            })
            .then(data => {
                $(".productImages").html(data)
            })


    })



    let isMain = $('#IsMain').is(':checked');

    if (isMain) {
        $('#fileInput').removeClass('d-none');
        $('#parentList').addClass('d-none');
    } else {
        $('#fileInput').addClass('d-none');
        $('#parentList').removeClass('d-none');
    }

    $('#IsMain').click(function () {
        let isMain = $(this).is(':checked');

        if (isMain) {
            $('#fileInput').removeClass('d-none');
            $('#parentList').addClass('d-none');
        } else {
            $('#fileInput').addClass('d-none');
            $('#parentList').removeClass('d-none');
        }
    })
})