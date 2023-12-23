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
        url: "/DanhMuc/NganKe/change-page",
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
        url: "/DanhMuc/NganKe/change-page",
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

$('#formTimKiem').on('submit', function (e) {
    var key = $('#search').val();

    if (key !== "") {

        $("#btnSearch").replaceWith('<button onclick="goBack()" title="Quay lại" id="btnBack" type="button" class="btn btn-light m-2"><i class="lni lni-angle-double-left d-flex"></i></button>')
        pages.remove();

        $('#tBody').empty();
        $("#loader").show();

        $.ajax({
            type: "post",
            url: "/DanhMuc/NganKe/searchTableNganKe",
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

function showModalEdit(id) {
    idModel = id;
    $.ajax({
        type: "get",
        url: "/DanhMuc/NganKe/show-modal/" + id,
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
$('#btnModal').on('click', function (e) {
    var form = document.getElementById('formUpdate');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
    } else {
        form.classList.remove('was-validated');
        fetch("/DanhMuc/NganKe/update-nganke", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Id: idModel,
                TenNganKe: $('#Ten').val()
            })
        })
            .then(Response => Response.json())
            .then(data => {
                $("#loader").show();
                $('#tBody').empty();
                $('#staticBackdrop').modal('hide');

                if ($('#search').val() === "") {
                    $.ajax({
                        type: "post",
                        url: "/DanhMuc/NganKe/change-page",
                        data: "active=" + active + "&page=" + page,
                        success: function (result) {
                            $('#toast').addClass(data.color);
                            $('#toastContent').text(data.message);
                            $('#toast').show();

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
                        url: "/DanhMuc/NganKe/searchTableNganKe",
                        data: "active=" + active + "&key=" + $('#search').val(),
                        success: function (result) {
                            $('#toast').addClass(data.color);
                            $('#toastContent').text(data.message);
                            $('#toast').show();

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
                    $('#toast').removeClass(data.color);
                }, 5000);
            })
            .catch(error => console.error(error))
    }
});

function deleteVT(id) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $("#loader").show();
        $('#tBody').empty();
        $.ajax({
            type: "post",
            url: "/DanhMuc/NganKe/remove-nganke",
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
        url: "/DanhMuc/NganKe/change-page",
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