function showImageModal(productId) {
    $.ajax({
        url: getImagesUrl,
        type: 'POST',
        data: {
            productId: productId,
            isStoreProduct: IsStoreProduct
        },
        success: function (data) {
            $('#imageModalBody').html(data);
            $('#imageModal').modal('show');
        },
        error: function () {
            alert('Failed to load images.');
        }
    });
}

function submitCreateImageForm() {
    var formData = new FormData($('#createImageForm')[0]);

    $.ajax({
        url: createImagesUrl,
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: data.message
                }).then(function () {
                    location.reload();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: data.message
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while uploading the images.'
            });
        }
    });
}