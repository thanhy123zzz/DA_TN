var idModel;
$(document).on('click', '#tBodyNHH tr', function () {
    // loại bỏ lớp active từ tất cả các hàng
    $('#tBodyNHH tr').removeClass('table-active');
    // thêm lớp active cho hàng được nhấp vào
    $(this).addClass('table-active');

    var id = this.getAttribute('data-id')

    $.ajax({
        type: "post",
        url: "/QuyDinh/GiaTheoNHH/load-gtnhh",
        data: "idNhh=" + id,
        success: function (result) {
            $('#tBodyGTNHH').replaceWith(result);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});
$('#search').on('keyup', function (e){
    var key = $(this).val();
    $('#tBodyGTNHH').empty();
    var listItem = $('#tBodyNHH tr');

    listItem.filter(function () {
        var have = $(this).text().toLowerCase().indexOf(key) > -1;
        $(this).toggle(have);
    });
});
function getValue(str) {
    return Number(str.replace(/[^0-9.-]+/g, ""));
}
function checkNumber(str) {
    return /[0-9,.\-$]+/.test(str);
}
function showModalGTNHH(idGtnhh) {
    let idNHH = $('.table-active').data('id');
    if (idNHH !== undefined) {
        $('#staticBackdrop').modal('show');
        idModel = idGtnhh;
        $.ajax({
            type: "post",
            url: "/QuyDinh/GiaTheoNHH/show-modal-gtnhh",
            data: "idGtnhh= " + idGtnhh + "&idNhh=" + idNHH,
            success: function (result) {
                $('#contentModal').empty();
                $('#contentModal').append(result.view);
                $('#staticBackdropLabel').text(result.title);
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
}
$('#btnModal').on('click', function (e) {
    var form = document.getElementById('formUpdate');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
    } else {
        form.classList.remove('was-validated');
        fetch("/QuyDinh/GiaTheoNHH/update-gtnhh", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Id: idModel,
                IdNhh: $('.table-active').attr('data-id'),
                TiLeLe: getValue($('#TLLe').val()),
                TiLeSi: getValue($('#TLSi').val()),
                Min: getValue($('#min').val()),
                Max: getValue($('#max').val())
            })
        })
            .then(Response => Response.json())
            .then(data => {
                if (data.statusCode == 200) {
                    $('#tBodyGTNHH').replaceWith(data.viewData);
                    $('#staticBackdrop').modal('hide');
                }
                $('#toast').addClass(data.color);
                $('#toastContent').text(data.message);
                $('#toast').show();

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(data.color);
                }, 5000);
            })
            .catch(error => console.error(error))
    }
});
function deleteGTNHH(id) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $('#tBodyGTNHH').empty();
        $.ajax({
            type: "post",
            url: "/QuyDinh/GiaTheoNHH/remove",
            data: "idGtnhh=" + id,
            success: function (result) {
                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                $('#tBodyGTNHH').replaceWith(result.viewData);

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(data.color);
                }, 5000);
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
}