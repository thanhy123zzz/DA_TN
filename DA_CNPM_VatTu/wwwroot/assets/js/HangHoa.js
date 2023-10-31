
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
$(document).on("click", '#nextPage', (e) => {

    $('#tBody').empty();
    $("#loader").show();
    page += 1;

    $.ajax({
        type: "post",
        url: "/DanhMuc/HangHoa/change-page",
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
        url: "/DanhMuc/HangHoa/change-page",
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
        url: "/DanhMuc/HangHoa/show-modal/" + id,
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

$(document).on('change', '#Avatar', function (e) {

    URL.revokeObjectURL($('#previewImage').prop('src'))

    const file = e.target.files[0];
    file.src = URL.createObjectURL(file);

    $('#previewImage').prop('src', file.src);
})

$('#formTimKiem').on('submit', function (e) {
    var key = $('#search').val();

    if (key !== "") {

        $("#btnSearch").replaceWith('<button onclick="goBack()" title="Quay lại" id="btnBack" type="button" class="btn btn-light m-2"><i class="lni lni-angle-double-left d-flex"></i></button>')
        pages.remove();

        $('#tBody').empty();
        $("#loader").show();

        $.ajax({
            type: "post",
            url: "/DanhMuc/HangHoa/searchTableHH",
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

$(document).on('focus', '.my-selectize, .input-style-1 input[type="text"]', function () {
    $('.my-selectize').each(function () {
        $(this).removeClass('is-invalid');
    });
    $('.input-style-1 input[type="text"]').each(function () {
        $(this).removeClass('is-invalid');
    });
});

$('#btnModal').on('click', function (e) {
    var invalid = false;
    $('.my-selectize').each(function () {
        if (this.selectize) {
            if ($(this).val() === "") {
                $(this).nextAll().addClass("is-invalid");
                invalid = true;
            }
        }
    });
    $('.input-style-1 input[type="text"]').each(function () {
        if ($(this).val() === "") {
            $(this).addClass("is-invalid")
            invalid = true;
        }
    });
    if (invalid) {
        return;
    }

    var image = $('#Avatar').get(0).files[0];
    var formData = new FormData();
    formData.append('FormFile', image);

    formData.append('HangHoa.Id', idModel);
    formData.append('HangHoa.MaHh', $('#Ma').val());
    formData.append('HangHoa.TenHh', $('#Ten').val());
    formData.append('HangHoa.Idnsx', $('#Nsx').val());
    formData.append('HangHoa.Iddvtchinh', $('#Dvtc').val());
    formData.append('HangHoa.Idhsx', $('#Hsx').val());
    formData.append('HangHoa.Idnhh', $('#Nhh').val());
    formData.append('HangHoa.IdBaoHanh', $('#Bh').val());

    $.ajax({
        url: "/DanhMuc/HangHoa/update-hh",
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
                    url: "/DanhMuc/HangHoa/change-page",
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
                    url: "/DanhMuc/HangHoa/searchTableHH",
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

});

function deleteHH(id) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $("#loader").show();
        $('#tBody').empty();
        $.ajax({
            type: "post",
            url: "/DanhMuc/HangHoa/remove",
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
        url: "/DanhMuc/HangHoa/change-page",
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