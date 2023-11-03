$(document).ready(function () {
    var datas = [
        {
            el: $('#KhachHangLS'),
            placeholder: '-- Khách hàng --',
            url: '/QuanLy/XuatKho/api/khs'
        }
    ];
    configCb(datas);
    configDate();
    configDateTime();
    formatNumberFloatWithElement($('.input-number-float'));

    loadTable();

    $.ajax({
        url: '/QuanLy/XuatKho/api/khs',
        method: 'POST',
    }).done(function (response) {
        $('#KhachHang').selectize({
            maxOptions: 50,
            valueField: "id",
            labelField: "ten",
            searchField: ["ten", "ma"],
            placeholder: '-- Khách hàng --',
            loadThrottle: 400,
            options: response,
            onChange: function (value) {
                $('#tBodyCtpx tr').each(function () {
                    var tr = $(this);
                    var idHh = tr.find('select[name="Idhh"]').val();
                    if (idHh) {
                        loadDonGia(idHh, tr.find('select[name="Iddvt"]').val(), value, tr.find('input[name="DonGia"]'));
                    }
                });
            },
        });
    });
    
    $(document).on('keyup', 'input[name="Sl"]', function () {
        var tr = $(this).closest('tr');
        var soLuongNhap = parseFloat($(this).inputmask('unmaskedvalue'));
        var donGiaNhap = parseFloat(tr.find('input[name="DonGia"]').inputmask('unmaskedvalue'));

        // thay đổi thành tiền
        if (donGiaNhap != "") {
            tr.find('input.ThanhTien').val(soLuongNhap * donGiaNhap);
            updateGiaTongTien();
        }
    });
    $(document).on('keyup', 'input[name="DonGia"]', function () {
        var tr = $(this).closest('tr');
        var donGiaNhap = parseFloat($(this).inputmask('unmaskedvalue'));
        var soLuongNhap = parseFloat(tr.find('input[name="Sl"]').inputmask('unmaskedvalue'));

        if (soLuongNhap != "") {
            tr.find('input.ThanhTien').val(soLuongNhap * donGiaNhap);
            updateGiaTongTien();
        }
    });
    $(document).on('keyup', 'input[name="Cktm"], input[name="Thue"]', function () {
        updateGiaTongTien();
    });
    $('#btnTaoPhieu').on('click', function () {
        var form = document.getElementById('formTaoPhieuXuat');
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
        } else {
            if (!$('#KhachHang').val() || !$('#NgayXuat').val()) {
                if (!$('#nhaCC').val()) {
                    showToast("Chưa chọn khách hàng!", 500);
                }
                else {
                    showToast("Chưa nhập ngày xuất!", 500);
                }
                return;
            }
            spinnerBtn($('#btnTaoPhieu'));
            // Lắng nghe sự kiện click trên nút có class "action-button"
            var phieuXuatCts = [];
            var check = true;
            $("#tBodyCtpx tr").each(function () {
                var tr = $(this);
                var idHh = tr.find('select[name="Idhh"]').val();
                console.log(idHh)
                var slXuat = tr.find('input[name="Sl"]').inputmask('unmaskedvalue');
                if (idHh) {
                    tr.removeClass('bg-danger');
                    var chiecKhau = tr.find('input[name="Cktm"]').inputmask('unmaskedvalue');
                    var thueVat = tr.find('input[name="Thue"]').inputmask('unmaskedvalue');
                    if (slXuat) {
                        tr.find('input[name="Sl"]').closest('td').removeClass('bg-danger');

                        var data = getDataFromTr(tr);
                        phieuXuatCts.push(data);
                    } else {
                        if (!slXuat) {
                            tr.find('input[name="Sl"]').closest('td').addClass('bg-danger');
                        }
                        check = false;
                        showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                    }
                } else {
                    if (!tr.is(":last-child")) {
                        tr.addClass('bg-danger');
                    }
                }
            });
            if (phieuXuatCts.length == 0) {
                check = false;
            }
            if (check) {
                var phieuXuat = {
                    Idkh: $('#KhachHang').val(),
                    SoHd: $('#soHD').val(),
                    NgayTao: $('#NgayXuat').val(),
                    NgayHd: $('#ngayHD').val(),
                    GhiChu: $('#ghiChu').val(),
                    ChiTietPhieuXuatMaps: phieuXuatCts,
                }
                $.ajax({
                    type: "post",
                    url: "/QuanLy/XuatKho/add-px",
                    data: JSON.stringify(phieuXuat),
                    contentType: "application/json",
                    success: function (response) {
                        showToast(response.message, response.statusCode);
                        showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                        xoaTrangPhieuXuatKho();
                    },
                    error: function (error) {
                        console.log(error);
                        showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                    }
                });
            } else {
                showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
            }

        }
    });
    $('#btnXoaTrang').on('click', function () {
        xoaTrangPhieuXuatKho();
    });

    $('#btnLoadLsXuat').on('click', function () {
        showLoader($('#LichSuXuat'));

        var toDay = $('#toDay').val();
        var fromDay = $('#fromDay').val();
        var KhLs = $('#KhachHangLS').val();
        var hhLS = $('#hangHoaLS').val();
        var soPhieuLS = $("#soPhieuLS").val();
        var soHDLS = $('#soHDLS').val();
        $.ajax({
            type: "post",
            url: "/QuanLy/XuatKho/loadTableLichSuXuat",
            data: "toDay=" + toDay + "&fromDay=" + fromDay + "&KhLs="
                + KhLs + "&hhLS=" + hhLS + "&soPhieuLS=" + soPhieuLS + "&soHDLS=" + soHDLS,
            success: function (result) {
                hideLoader();
                $('#tBodyLSXuat').empty();
                $('#tBodyLSXuat').append(result);
            },
            error: function () {
                alert("Fail");
            }
        });
    });

    $(document).on('click', '.btn-view-pn', function () {
        $('#bordered-justified-profile').removeClass('active');
        $('#bordered-justified-profile').removeClass('show');
        $('#tabXemPhieu').removeClass('d-lg-none');
        var id = $(this).val();
        $.ajax({
            type: "post",
            url: "/QuanLy/XuatKho/ViewThongTinPhieuXuat",
            data: "idPX=" + id,
            success: function (result) {
                $('#tabXemPhieu').replaceWith(result);
            },
            error: function () {
                alert("Fail");
            }
        });
    });
});
function loadTable() {
    $.ajax({
        type: 'POST',
        url: '/QuanLy/XuatKho/api/hhs'
    }).done(function (response) {
        $('#tBodyCtpx').append(getRowPhieuXuatCt());
        configRowPhieuXuatCt($('#tBodyCtpx tr:last'), response);

        $('#hangHoaLS').selectize({
            maxOptions: 50,
            valueField: "id",
            labelField: "ten",
            searchField: ["ten", "ma"],
            placeholder: "-- Hàng hoá --",
            loadThrottle: 400,
            options: response,
            render: {
                option: function (item, escape) {
                    return `<div class="px-2 py-1"><b>[${item.ma}]</b> - ${item.ten}</div>`;
                },
                no_results: function (data, escape) {
                    return '<div class="no-results">Không tìm thấy dữ liệu </div>';
                },
            },
        });
    });
}
function xoaTrangPhieuXuatKho() {
    $.ajax({
        type: "post",
        url: "/QuanLy/XuatKho/api/getSoPhieuXuat",
        success: function (result) {
            $('#inputText').val(result);
        },
        error: function () {
            alert("Fail");
        }
    });
    $('#KhachHang')[0].selectize.clear();
    $('#NgayXuat').val(getDateTimeNow());
    $('#ngayHD').val(getDateNow());
    $('#soHD').val('');
    $('#ghiChu').val('');

    $('#tBodyCtpx tr').each(function () {
        var tr = $(this);

        tr.find('select').each(function () {
            if (this.selectize) {
                this.selectize.destroy();
            }
        });
    });
    $('#tBodyCtpx').empty();
    loadTable();
}
function getRowPhieuXuatCt() {
    return `<tr>
        <td><select class="form-select form-table" name="Idhh" style="width: 300px;"></select></td>
        <td class="text-center"><img class='image-modal object-fit-cover' style="max-height: 32px;max-width: 42px;border-radius:0;" src="" alt=''></td>
        <td><select class="form-select form-table" name="Iddvt" style="width: 140px;"></select></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Sl" style="min-width: 80px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="DonGia" readonly style="min-width: 120px;"/></td>
        <td><input autocomplete="off" class="form-control form-table ThanhTien input-number-float" readonly style="min-width: 140px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Cktm" style="min-width: 60px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Thue" style="min-width: 60px;"/></td>
        <td class='last-td-column'>
            <div class="action justify-content-center">
                <button class="text-danger btn-remove-ct">
                    <i class="lni lni-trash-can"></i>
                </button>
            </div>
        </td>
    </tr>`;
    
}
function configRowPhieuXuatCt(tr, hhs) {
    var cbHangHoa = tr.find('select[name="Idhh"]');
    cbHangHoa.selectize({
        maxOptions: 50,
        valueField: "id",
        labelField: "ten",
        searchField: ["ten", "ma"],
        placeholder: '-- Hàng hoá --',
        options: hhs,
        loadThrottle: 400,
        dropdownParent: '#dropdow-show',
        closeAfterSelect: 0,
        allowEmptyOption: false,
        onDropdownOpen: function ($dropdown) {
            showDropdownMenu(cbHangHoa, $dropdown);
        },
        onChange: function (value) {
            dropDownHhChange(cbHangHoa, value);
        },
        onFocus: function ($dropdown) {
            $('.my-selectize-2').not(this.$input).each(function () {
                if (this.selectize) {
                    this.selectize.close();
                    this.selectize.blur();
                }
            });
        },
        render: {
            option: function (item, escape) {
                return `<div class="px-2 py-1">
                            <div class="d-flex justify-content-between">
                                <div class="col-auto">
                                    <b>[${item.ma}]</b>
                                </div>
                                <div class="col-auto text-end">
                                ${item.ten}
                                </div>
                           </div>
                           <div>
                                Số lượng còn: ${item.slTon}
                           </div>
                        </div>`;
            },
            no_results: function (data, escape) {
                return '<div class="no-results">Không tìm thấy dữ liệu </div>';
            },
        },
    });
}
function dropDownHhChange(cbHangHoa, value) {
    var tr = cbHangHoa.closest('tr');
    var selectize = cbHangHoa[0].selectize;
    if (selectize && value) {
        
        var options = Object.values(selectize.options);
        var option = options.find(function (item) {
            return item.id == value;
        });
        tr.find('img.image-modal').prop('alt', option.ten);
        if (option.hinh) {
            tr.find('img.image-modal').prop('src', option.hinh);
        }
        tr.find('input[name="Sl"]').prop('max', option.slTon);
        if (option.slTon == 0) {
            showToast("Đã hết hàng trong kho!", 500);
        }
        formatNumberFloatWithElement(tr.find('.input-number-float'));
        configDateLongMask(tr.find('.date-sort-mask'));

        option.dvts.unshift(option.dvtChinh);
        var cbDvt = tr.find('select[name="Iddvt"]');

        loadDonGia(value, option.dvtChinh.id, $('#KhachHang').val(), tr.find('input[name="DonGia"]'));

        if (cbDvt[0].selectize) {
            cbDvt[0].selectize.clear();
            cbDvt[0].selectize.clearOptions();
            cbDvt[0].selectize.addOption(option.dvts);
            cbDvt[0].selectize.setValue(option.dvtChinh.id);
        } else {
            cbDvt.selectize({
                placeholder: "-- Đơn vị tính --",
                options: option.dvts,
                valueField: "id",
                labelField: "ten",
                searchField: ["ten", "ma"],
                dropdownParent: '#dropdow-show',
                closeAfterSelect: 0,
                items: [option.dvtChinh.id],
                onDropdownOpen: function ($dropdown) {
                    showDropdownMenu(cbDvt, $dropdown);
                },
                onChange: function (value) {
                    var idHh = tr.find('select[name="Idhh"]').val();
                    loadDonGia(idHh, value, $('#KhachHang').val(), tr.find('input[name="DonGia"]'));
                },
                onFocus: function ($dropdown) {
                    $('.my-selectize-2').not(this.$input).each(function () {
                        if (this.selectize) {
                            this.selectize.close();
                            this.selectize.blur();
                        }
                    });
                },
            });
        }
        if (tr.is(':last-child')) {
            $('#tBodyCtpx').append(getRowPhieuXuatCt());
            configRowPhieuXuatCt($('#tBodyCtpx tr:last'), options);
        }
    }
}
function loadDonGia(idHh, idDvt, idKh, input) {
    console.log({
        idKh: idKh,
        idHh: idHh,
        idDvt: idDvt
    })
    if (idKh) {
        if (idHh && idDvt) {
            $.ajax({
                type: "post",
                url: "/QuanLy/XuatKho/load-tthh",
                data: {
                    idKh: idKh,
                    idHh: idHh,
                    idDvt: idDvt
                },
                success: function (result) {
                    input.val(result).trigger('keyup');
                },
                error: function (loi) {
                    console.log(loi);
                }
            });
        }
    }
}

function updateGiaTongTien() {

    var tongTienHang = 0;
    var tongTienCK = 0;
    var tongTienThue = 0;
    var tongTra = 0;
    $('#tBodyCtpx tr').each(function () {
        var tr = $(this);

        var SlxGia = parseFloat(tr.find('input.ThanhTien').inputmask('unmaskedvalue'));

        if (SlxGia) {
            tongTienHang += parseFloat(SlxGia);
        }

        var chiecKhau = parseFloat(tr.find('input[name="Cktm"]').inputmask('unmaskedvalue'));
        var tienChiecKhau = 0;
        if (chiecKhau && SlxGia) {
            tienChiecKhau = ((chiecKhau * SlxGia) / 100);
            tongTienCK += tienChiecKhau;
        }

        var thueVat = parseFloat(tr.find('input[name="Thue"]').inputmask('unmaskedvalue'));
        var tienThue = 0;
        if (thueVat && SlxGia) {
            tienThue = (((SlxGia - tienChiecKhau) * thueVat) / 100);
            tongTienThue += tienThue;
        }
    })
    $('#TienHang').val(tongTienHang);
    $('#TienCK').val(tongTienCK);
    $('#TienThue').val(tongTienThue);
    $('#TienThanhToan').val(tongTienHang - tongTienCK + tongTienThue);
}
function cancelXemPhieu() {
    $('#tabXemPhieu').addClass('d-lg-none');
    $('#bordered-justified-profile').addClass('active');
    $('#bordered-justified-profile').addClass('show');
}