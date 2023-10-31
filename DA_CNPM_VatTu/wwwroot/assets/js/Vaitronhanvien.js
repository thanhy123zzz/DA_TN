$(document).on('click', '#tBodyNhanVien tr', function () {
    // loại bỏ lớp active từ tất cả các hàng
    $('#tBodyNhanVien tr').removeClass('table-active');
    // thêm lớp active cho hàng được nhấp vào
    $(this).addClass('table-active');

    var id = this.getAttribute('data-id')

    $.ajax({
        type: "post",
        url: "/HeThong/VaiTroNhanVien/load-vaitro",
        data: "idNv=" + id,
        success: function (result) {
            $('#tBodyPhanQuyen').replaceWith(result);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});
$('#searchNV').on('keyup', (e) => {
    var key = e.target.value;

    $.ajax({
        type: "post",
        url: "/HeThong/VaiTroNhanVien/search-nv",
        data: "key=" + key,
        success: function (result) {
            $('#tBodyNhanVien').empty();

            result.data.map(item => $('#tBodyNhanVien').append(`<tr data-id="${item.id}" style="cursor:pointer"><td>${item.maNv}</td><td>${item.tenNv}</td></tr>`));

        },
        error: function (loi) {
            console.log(loi);
        }
    });
});

function showModalVaiTro(idVT) {
    let idNv = $('.table-active').data('id');
    if (idNv !== undefined) {
        $('#staticBackdrop').modal('show');
        $.ajax({
            type: "post",
            url: "/HeThong/VaiTroNhanVien/show-modal-vaitro",
            data: "idNv= " + idNv + "&idVt=" + idVT,
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
    $.ajax({
        type: "post",
        url: "/HeThong/VaiTroNhanVien/update-vaitro-nhanvien",
        data: "idVt=" + $('#roles').val() + "&idNv=" + $('.table-active').data('id'),
        success: function (result) {
            if (result.statusCode == 200) {
                $('#staticBackdrop').modal('hide');
                $('#tBodyPhanQuyen').replaceWith(result.viewData);

                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(result.color);
                }, 5000);
            }
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});

function deletePhanQuyen(idpqnv) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $.ajax({
            type: "post",
            url: "/HeThong/VaiTroNhanVien/delete-vaitro-nhanvien",
            data: "idPqnv=" + idpqnv,
            success: function (result) {
                if (result.statusCode == 200) {
                    $('#tBodyPhanQuyen').empty();
                    $('#tBodyPhanQuyen').replaceWith(result.viewData);

                    $('#toast').addClass(result.color);
                    $('#toastContent').text(result.message);
                    $('#toast').show();

                    setTimeout(function () {
                        $('#toast').hide();
                        $('#toast').removeClass(result.color);
                    }, 5000);
                }
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
}