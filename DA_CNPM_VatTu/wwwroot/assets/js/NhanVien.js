﻿
$(document).on('change', '#Avatar', function (e) {

    URL.revokeObjectURL($('#previewImage').prop('src'))

    const file = e.target.files[0];
    file.src = URL.createObjectURL(file);

    $('#previewImage').prop('src', file.src);
})
$("#loader").hide();
const pages = $("#pages");
var contentTable = $('#tBody').children();
var page = 0;
var idModel = 0;
var active = $('#active').val();
if ($('#tBody').children().length < 10) {
    $('#nextPage').addClass('disabled')
} else {
    $('#nextPage').removeClass('disabled')
}

$('#formTimKiem').on('submit', function (e) {
    var key = $('#search').val();
    if (key !== "") {

        $("#btnSearch").replaceWith('<button onclick="goBack()" title="Quay lại" id="btnBack" type="button" class="btn btn-light m-2"><i class="lni lni-angle-double-left d-flex"></i></button>')
        pages.remove();

        $('#tBody').empty();
        $("#loader").show();

        $.ajax({
            type: "post",
            url: "/DanhMuc/NhanVien/searchTableNV",
            data: "active=" + active + "&key=" + key,
            success: function (result) {
                $("#loader").hide();
                $('#tBody').empty();
                $('#tBody').append(result);

            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
    e.preventDefault();
});
function goBack() {
    $('#tBody').empty()
    $('#search').val('');
    $("#content").append(pages);

    $('#tBody').append(contentTable)

    $("#btnBack").replaceWith('<button title=Tìm kiếm" id="btnSearch" type="submit" class="btn btn-light m-2"><i class="lni lni-keyword-research d-flex"></i></button>')
};

$(document).on("click", '#nextPage', (e) => {

    $('#tBody').empty();
    $("#loader").show();
    page += 1;

    $.ajax({
        type: "post",
        url: "/DanhMuc/NhanVien/change-page",
        data: "active=" + active + "&page=" + page,
        success: function (result) {
            $("#loader").hide();
            $('#tBody').empty();
            $('#tBody').append(result);

            contentTable = $('#tBody').children();
            $('#currentPage').text(page + 1);
            if ($('#tBody').children().length < 10) {
                $('#nextPage').addClass('disabled')
            } else {
                $('#nextPage').removeClass('disabled')
            }
            if (page > 0) {
                $('#prePage').removeClass('disabled')
            } else {
                $('#prePage').addClass('disabled')
            }

        },
        error: function (loi) {
            console.log(loi);
        }
    });
    e.preventDefault();
});
$(document).on('click', '#prePage', (e) => {

    $('#tBody').empty();
    $("#loader").show();
    page -= 1;

    $.ajax({
        type: "post",
        url: "/DanhMuc/NhanVien/change-page",
        data: "active=" + active + "&page=" + page,
        success: function (result) {
            $("#loader").hide();
            $('#tBody').empty();
            $('#tBody').append(result);
            contentTable = $('#tBody').children();
            $('#currentPage').text(page + 1);

            if ($('#tBody').children().length < 10) {
                $('#nextPage').addClass('disabled')
            } else {
                $('#nextPage').removeClass('disabled')
            }
            if (page > 0) {
                $('#prePage').removeClass('disabled')
            } else {
                $('#prePage').addClass('disabled')
            }
        },
        error: function (loi) {
            console.log(loi);
        }
    });
    e.preventDefault();
});

function showModalEdit(id) {
    idModel = id;
    $.ajax({
        type: "get",
        url: "/DanhMuc/NhanVien/show-modal/" + id,
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

$(document).on('focus', '.my-selectize, .input-style-1 input[type="text"]', function () {
    $('.my-selectize').each(function () {
        $(this).removeClass('is-invalid');
    });
    $('.input-style-1 input[type="text"]').each(function () {
        $(this).removeClass('is-invalid');
    });
});

$('#btnModal').on('click', function (e) {
    var form = document.getElementById('formUpdate');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        document.getElementById('Nnv').selectize.$control[0].classList.add('is-invalid');
    } else {
        form.classList.remove('was-validated');
        document.getElementById('Nnv').selectize.$control[0].classList.remove('is-invalid');
        var image = $('#Avatar').get(0).files[0];
        var formData = new FormData();
        formData.append('FormFile', image);

        formData.append('NhanVien.Id', idModel);
        formData.append('NhanVien.MaNv', $('#Ma').val());
        formData.append('NhanVien.TenNv', $('#Ten').val());
        formData.append('NhanVien.DiaChi', $('#DiaChi').val());
        formData.append('NhanVien.Email', $('#Email').val());
        formData.append('NhanVien.Sdt', $('#Sdt').val());
        formData.append('NhanVien.QueQuan', $('#QueQuan').val());
        formData.append('NhanVien.Cccd', $('#Cccd').val());
        formData.append('NhanVien.GioiTinh', $('#GioiTinh').val());
        formData.append('NgaySinh', $('#NgaySinh').val());
        formData.append('NhanVien.Idnnv', $('#Nnv').val());
        formData.append('Account.TaiKhoan', $('#TaiKhoan').val());
        formData.append('Account.MatKhau', $('#MatKhau').val());

        $.ajax({
            url: "/DanhMuc/NhanVien/update-nv",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success: function (result) {
                $("#loader").show();
                $('#tBody').empty();
                $('#staticBackdrop').modal('hide');

                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                if ($('#search').val() === "") {
                    $.ajax({
                        type: "post",
                        url: "/DanhMuc/NhanVien/change-page",
                        data: "active=" + active + "&page=" + page,
                        success: function (result) {

                            $('#tBody').append(result);
                            $("#loader").hide();
                        },
                        error: function (loi) {
                            console.log(loi);
                        }
                    });
                }
                else {
                    $.ajax({
                        type: "post",
                        url: "/DanhMuc/NhanVien/searchTableNV",
                        data: "active=" + active + "&key=" + $('#search').val(),
                        success: function (result) {
                            $("#loader").hide();
                            $('#tBody').append(result);
                        },
                        error: function (loi) {
                            console.log(loi);
                        }
                    });
                }

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(result.color);
                }, 5000);
            },
            error: function (loi) {
                console.log(loi)
            }
        });
    }
});

function deleteNV(id) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $("#loader").show();
        $('#tBody').empty();
        $.ajax({
            type: "post",
            url: "/DanhMuc/NhanVien/remove",
            data: "Id=" + id + "&active=" + active + "&page=" + page + "&key=" + $('#search').val(),
            success: function (result) {

                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                $('#tBody').append(result.nsxs);
                $("#loader").hide();
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
function changeActive() {
    $("#loader").show();
    $('#tBody').empty();
    active = $('#active').val();

    $.ajax({
        type: "post",
        url: "/DanhMuc/NhanVien/change-page",
        data: "active=" + active + "&page=" + page,
        success: function (result) {
            $("#loader").hide();
            $('#tBody').append(result);
            contentTable = $('#tBody').children();
        },
        error: function (loi) {
            console.log(loi);
        }
    });
}