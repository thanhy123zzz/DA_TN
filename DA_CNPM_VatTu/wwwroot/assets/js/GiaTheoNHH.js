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
$('#search').on('keyup', (e) => {
    var key = e.target.value;
    $('#tBodyGTNHH').empty();
    $.ajax({
        type: "post",
        url: "/QuyDinh/GiaTheoNHH/search-nhh",
        data: "key=" + key,
        success: function (result) {
            $('#tBodyNHH').empty();

            result.data.map(item => $('#tBodyNHH').append(`<tr data-id="${item.id}" style="cursor:pointer"><td class="first-td-column">${item.maNhh}</td><td>${item.tenNhh}</td></tr>`));

        },
        error: function (loi) {
            console.log(loi);
        }
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
$('#btnModal').on('click', (e) => {
    if ($('#TLLe').val() === "" && $('#TLSi').val() === "") {
        $('.text-decimal').addClass("is-invalid");
        return false;
    }

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