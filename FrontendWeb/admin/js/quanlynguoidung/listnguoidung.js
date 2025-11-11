// =========================
// ⚙️ Cấu hình API
// =========================
const userBaseUrl = 'http://localhost:5150/api/NguoiDung';

// =========================
// 🔧 Hàm fetch JSON
// =========================
async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    const text = await res.text();
    if (!res.ok) {
        let message = text;
        try {
            const json = JSON.parse(text);
            message = json.message || JSON.stringify(json);
        } catch {}
        throw new Error(message);
    }
    return text ? JSON.parse(text) : {};
}

// =========================
// 🔒 Escape HTML
// =========================
function escapeHtml(str) {
    return str
        ? str.replace(/[&<>"']/g, t => (
            { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[t]
        ))
        : '';
}

// =========================
// 👥 Load danh sách người dùng
// =========================
async function loadUsers() {
    const users = await fetchJson(userBaseUrl);
    const table = $('#usersTable').DataTable();

    // Xóa toàn bộ dữ liệu cũ trong bảng
    table.clear();

    const viTriMap = {
        'admin': 'Admin',
        'nhan vien': 'Nhân viên',
        'khachhang': 'Khách hàng'
    };

    // Thêm dữ liệu mới
    users.forEach(u => {
        table.row.add([
            u.id,   
            escapeHtml(u.hoTen),
            escapeHtml(u.tenDn),
            escapeHtml(u.email),
            escapeHtml(u.sdt.replace(/(\d{4})(?=\d)/g, '$1 ')),
            escapeHtml(viTriMap[u.viTri?.toLowerCase()] || u.viTri),
            (() => {
                const status = (u.trangThai || '').toLowerCase();
                let colorClass = 'badge-secondary';
                let text = escapeHtml(u.trangThai);
    
                if (status.includes('đang')) colorClass = 'badge-success';
                else if (status.includes('ngừng')) colorClass = 'badge-danger';
    
                return `<span class="badge ${colorClass} px-3 py-2">${text}</span>`;
            })(),
            `
            <button class="btn btn-sm btn-info edit-btn" data-id="${u.id}">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-warning password-btn" data-id="${u.id}" data-ten-dn="${u.tenDn}">
                <i class="fas fa-key"></i>
            </button>
            `
        ]);
    });

    // Cập nhật lại bảng
    table.draw();
}

// =========================
// 🏁 Khởi tạo DataTable (chỉ 1 lần)
// =========================
function initDataTable() {
    $('#usersTable').DataTable({
        order: [[0, 'desc']],
        autoWidth: true,  // ✅ Cho phép DataTables tự tính lại độ rộng
        responsive: true, // ✅ Co giãn linh hoạt trên mọi kích thước màn hình
        columnDefs: [
            { orderable: false, targets: -1 } // chỉ tắt sắp xếp ở cột Hành động
        ],
        drawCallback: function () {
            // ✅ Sau mỗi lần vẽ lại bảng, điều chỉnh lại cột
            this.api().columns.adjust();
        }
    });
}


// =========================
// ➕ Mở modal thêm mới
// =========================
function openModalForCreate() {
    document.getElementById('modalTitle').innerText = 'Thêm người dùng mới';
    document.getElementById('userForm').reset();
    document.getElementById('Id').value = '';

    document.getElementById('passwordGroup').style.display = 'block';
    document.getElementById('tenDnGroup').style.display = 'block';
    document.getElementById('trangThaiGroup').style.display = 'none';

    $('#userModal').modal('show');
}

// =========================
// ✏️ Chỉnh sửa người dùng
// =========================
async function editUser(id) {
    const u = await fetchJson(`${userBaseUrl}/${id}`);
    document.getElementById('modalTitle').innerText = 'Sửa thông tin người dùng';
    document.getElementById('userForm').reset();

    document.getElementById('Id').value = u.id;
    document.getElementById('HoTen').value = u.hoTen || '';
    document.getElementById('Email').value = u.email || '';
    document.getElementById('Sdt').value = u.sdt || '';
    document.getElementById('ViTri').value = u.viTri || 'khachhang';
    document.getElementById('TrangThai').value = u.trangThai || 'đang hoạt động';

    document.getElementById('passwordGroup').style.display = 'none';
    document.getElementById('tenDnGroup').style.display = 'none';
    document.getElementById('trangThaiGroup').style.display = 'block';

    $('#userModal').modal('show');
}

// =========================
// 💾 Lưu người dùng (POST/PUT)
// =========================
async function saveUser() {
    const id = document.getElementById('Id').value;

    const payload = id
        ? {
            hoTen: HoTen.value.trim(),
            email: Email.value.trim(),
            sdt: Sdt.value.trim(),
            viTri: ViTri.value.trim(),
            trangThai: TrangThai.value.trim()
        }
        : {
            hoTen: HoTen.value.trim(),
            tenDn: TenDn.value.trim(),
            matKhau: MatKhau.value,
            email: Email.value.trim(),
            sdt: Sdt.value.trim(),
            viTri: ViTri.value.trim()
        };

    const method = id ? 'PUT' : 'POST';
    const url = id ? `${userBaseUrl}/${id}` : userBaseUrl;

    await fetchJson(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    $('#userModal').modal('hide');
    alert(id ? 'Cập nhật thành công!' : 'Thêm mới thành công!');

    // 🔄 Gọi lại loadUsers sau khi lưu
    await loadUsers();
}

// =========================
// 🔐 Đổi mật khẩu
// =========================
async function changePassword() {
    const id = ChangePassId.value;
    const pass1 = NewMatKhau.value;
    const pass2 = NewMatKhauConfirm.value;

    if (pass1 !== pass2) return alert('Mật khẩu xác nhận không khớp!');

    await fetchJson(`${userBaseUrl}/${id}/doimatkhau`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ matKhauMoi: pass1 })
    });

    $('#changePasswordModal').modal('hide');
    alert('Đổi mật khẩu thành công!');
}

// =========================
// ⚡ Gắn sự kiện
// =========================
document.addEventListener('DOMContentLoaded', async () => {
    initDataTable();
    await loadUsers();

    document.getElementById('addUserBtn').addEventListener('click', openModalForCreate);
    document.getElementById('saveBtn').addEventListener('click', saveUser);
    document.getElementById('saveNewPassBtn').addEventListener('click', changePassword);

    $(document).on('click', '.edit-btn', function () {
        editUser($(this).data('id'));
    });

    $(document).on('click', '.delete-btn', function () {
        deleteUser($(this).data('id'));
    });

    $(document).on('click', '.password-btn', function () {
        document.getElementById('changePassModalTitle').innerText =
            `Đổi mật khẩu cho: ${$(this).data('ten-dn')}`;
        document.getElementById('ChangePassId').value = $(this).data('id');
        $('#changePasswordModal').modal('show');
    });
});
