var idModel;
$(document).ready(function () {
    onloadPage();
});
function getValue(str) {
    return Number(str.replace(/[^0-9.-]+/g, ""));
}
function onloadPage() {
    $.ajax({
        type: 'POST',
        url: '/QuanLy/NhapKho/api/hhs'
    }).done(function (response) {
        $('#hangHoa').selectize({
            maxOptions: 30,
            onChange: function (value) {
                if (value !== "") {
                    $.ajax({
                        type: "post",
                        url: "/QuyDinh/GiaTheoKH/load-hhdvt",
                        data: "idHh=" + value,
                        success: function (result) {
                            $('#tBodyDVT').replaceWith(result);
                        },
                        error: function (loi) {
                            console.log(loi);
                        }
                    });
                }
                if ($('#KhachHang').val() !== "") {
                    $.ajax({
                        type: "post",
                        url: "/QuyDinh/GiaTheoKH/load-gtkh",
                        data: "idHh=" + value + "&idKh=" + $('#KhachHang').val(),
                        success: function (result) {
                            $('#tBodyGTKH').replaceWith(result);
                        },
                        error: function (loi) {
                            console.log(loi);
                        }
                    });
                }
            },
            onFocus: function ($dropdown) {
                $('.my-selectize-2').not(this.$input).each(function () {
                    if (this.selectize) {
                        this.selectize.close();
                        this.selectize.blur();
                    }
                });
            },
            valueField: "id",
            labelField: "ten",
            searchField: ["ten", "ma"],
            placeholder: '-- Hàng hoá --',
            allowEmptyOption: false,
            loadThrottle: 400,
            options: response
        });
    });
    $.ajax({
        type: 'POST',
        url: '/QuyDinh/GiaTheoKH/api/khs'
    }).done(function (response) {
        $('#KhachHang').selectize({
            options: response,
            maxOptions: 50,
            onChange: function (value) {
                if (value !== "" && $('#hangHoa').val() !== "") {
                    $.ajax({
                        type: "post",
                        url: "/QuyDinh/GiaTheoKH/load-gtkh",
                        data: "idHh=" + $('#hangHoa').val() + "&idKh=" + value,
                        success: function (result) {
                            $('#tBodyGTKH').replaceWith(result);
                        },
                        error: function (loi) {
                            console.log(loi);
                        }
                    });
                }
            },
            onFocus: function ($dropdown) {
                $('.my-selectize-2').not(this.$input).each(function () {
                    if (this.selectize) {
                        this.selectize.close();
                        this.selectize.blur();
                    }
                });
            },
            valueField: "id",
            labelField: "ten",
            searchField: ["ten", "ma"],
            placeholder: '-- Khách hàng --',
            allowEmptyOption: false,
            loadThrottle: 400,
            render: {
                item: function (item, escape) {
                    return '<div>' + escape(item.ten + ' (' + item.loai + ')') + '</div>';
                },
                option: function (item, escape) {
                    return `<div class="px-2 py-1"><b>${item.ten}</b> - [${item.loai}]</div>`;
                }
            },
        });
    });
}

function showModal(idGBKH) {
    if ($('#hangHoa').val() !== "" && $('#KhachHang').val() !== "") {
        $('#staticBackdrop').modal('show');
        idModel = idGBKH;
        $.ajax({
            type: "post",
            url: "/QuyDinh/GiaTheoKH/show-modal",
            data: "idHh= " + $('#hangHoa').val() + "&idKh=" + $('#KhachHang').val() + "&idGTKH=" + idGBKH,
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
function toDecimal(str) {
    return parseFloat(str).toLocaleString('en-US', {
        style: 'decimal',
        maximumFractionDigits: 2,
        minimumFractionDigits: 2
    });
}
$('#btnModal').on('click', function (event) {
    if ($('#LoaiKh').val() == 1) {
        if ($('#TLSi').val() === "" && $('#GBSi').val() === "") {
            $('#TLSi').addClass("is-invalid");
            $('#GBSi').addClass("is-invalid");
            return false;
        }
    } else {
        if ($('#TLLe').val() === "" && $('#GBLe').val() === "") {
            $('#TLLe').addClass("is-invalid");
            $('#GBLe').addClass("is-invalid");
            return false;
        }
    }
    

    fetch("/QuyDinh/GiaTheoKH/update-gtkh", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Id: idModel,
            IdHh: $('#hangHoa').val(),
            IdKh: $('#KhachHang').val(),
            IdDvt: $('#DVT').val(),
            TiLeLe: getValue($('#TLLe').val()),
            TiLeSi: getValue($('#TLSi').val()),
            GiaBanLe: getValue($('#GBLe').val()),
            GiaBanSi: getValue($('#GBSi').val())
        })
    })
        .then(Response => Response.json())
        .then(data => {
            $('#staticBackdrop').modal('hide');
            if (data.statusCode == 200) {
                $('#tBodyGTKH').empty();
                $('#tBodyGTKH').replaceWith(data.viewData);
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

function deleteGTKH(idGTKH) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $('#tBodyGTKH').empty();
        $.ajax({
            type: "post",
            url: "/QuyDinh/GiaTheoKH/remove",
            data: "idGTKH=" + idGTKH,
            success: function (result) {
                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                $('#tBodyGTKH').replaceWith(result.viewData);

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