    // =========================
    // ⚙️ Cấu hình API
    // =========================
    const baseUrl = 'http://localhost:5150/api/DonHang';

    // Debug log
    console.log('[listdonhang] baseUrl =', baseUrl);

    // =========================
    // 🧩 Hàm tiện ích (ĐÃ SỬA)
    // =========================
    // Thêm tùy chọn để bỏ qua cache bằng cách thêm tham số timestamp
    async function fetchJson(url, options = {}, bypassCache = false) {
        let finalUrl = url;
        
        // [SỬA LỖI] Nếu bypassCache = true, thêm timestamp vào URL
        if (bypassCache) {
            const timestamp = new Date().getTime();
            finalUrl = `${url}${url.includes('?') ? '&' : '?'}_t=${timestamp}`;
        }

        const res = await fetch(finalUrl, options);
        
        if (!res.ok) {
            const text = await res.text();
            throw new Error(`${res.status} ${res.statusText} - ${text}`);
        }
        return res.json();
    }

    function formatCurrency(value) {
        if (value == null) return '';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(value);
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    // =========================
// 📦 Tải danh sách đơn hàng (ĐÃ SỬA TRIỆT ĐỂ CHO DATATABLES)
// =========================
let dataTableInstance = null; // Khai báo biến toàn cục để lưu trữ instance của DataTable

async function loadOrders() {
    try {
        // Luôn bỏ qua cache khi lấy dữ liệu
        const data = await fetchJson(baseUrl, {}, true); 
        
        // 1. CHUẨN BỊ DỮ LIỆU cho DataTables
        const dataForDataTable = data.map(d => [
            d.id,
            new Date(d.ngayDat).toLocaleDateString('vi-VN'),
            escapeHtml(d.tenNguoiDung || 'N/A'),
            escapeHtml(d.diaChiGiaoHang || 'N/A'),
            formatCurrency(d.tongTien),
            `<span class="badge ${getStatusBadgeClass(d.trangThai)}">${escapeHtml(d.trangThai)}</span>`,
            // Cột Hành động
            `
                <button class="btn btn-sm btn-info" onclick="viewOrder(${d.id})">
                    <i class="fas fa-eye"></i> Chi tiết
                </button>
                <button class="btn btn-sm btn-warning" onclick="showUpdateModal(${d.id})">
                    <i class="fas fa-sync"></i> Cập nhật
                </button>
            `
        ]);

        // 2. XỬ LÝ DATATABLES
        if ($.fn.DataTable.isDataTable('#ordersTable')) {
            // Nếu đã tồn tại, xóa hết dữ liệu cũ và thêm dữ liệu mới
            dataTableInstance.clear().rows.add(dataForDataTable).draw();
        } else {
            // Nếu chưa tồn tại, khởi tạo lần đầu
            dataTableInstance = $('#ordersTable').DataTable({
                data: dataForDataTable, // Truyền dữ liệu trực tiếp
                destroy: true, // Đảm bảo việc destroy/re-init trong tương lai hoạt động
                columns: [
                    { title: "ID" },
                    { title: "Ngày đặt" },
                    { title: "Người dùng" },
                    { title: "Địa chỉ" },
                    { title: "Tổng tiền" },
                    { title: "Trạng thái" },
                    { title: "Hành động" }
                ],
                language: {
                    url: 'https://cdn.datatables.net/plug-ins/2.0.7/i18n/vi.json'
                }
            });
        }

    } catch (err) {
        console.error('Load orders failed', err);
        showToast('Không thể tải danh sách đơn hàng: ' + err.message, 'danger');
        // Nếu có lỗi, đảm bảo bảng trống (dành cho lần khởi tạo đầu tiên)
        const tbody = document.getElementById('ordersBody');
        if (tbody) tbody.innerHTML = `
            <tr><td colspan="7" class="text-center text-danger">
                Lỗi tải dữ liệu: ${err.message}
            </td></tr>`;
    }
}

    // =========================
    // 🔍 Tìm kiếm đơn hàng theo ID
    // =========================
    async function searchOrder() {
        const id = document.getElementById('searchId').value.trim();
        if (!id) return showToast('Vui lòng nhập ID đơn hàng!', 'warning');
        try {
            const order = await fetchJson(`${baseUrl}/${id}`);
            showOrderDetails(order);
        } catch (err) {
            showToast('Không tìm thấy đơn hàng: ' + err.message, 'danger');
        }
    }

    // =========================
    // 🔎 Xem chi tiết đơn hàng
    // =========================
    async function viewOrder(id) {
        try {
            const order = await fetchJson(`${baseUrl}/${id}`);
            showOrderDetails(order);
        } catch (err) {
            console.error('View order failed', err);
            alert('Không thể xem chi tiết đơn hàng: ' + err.message);
        }
    }

    function showOrderDetails(order) {
        const div = document.getElementById('orderDetails');

        div.innerHTML = `
            <p><strong>ID:</strong> ${order.id}</p>
            <p><strong>Ngày đặt:</strong> ${new Date(order.ngayDat).toLocaleString('vi-VN')}</p>
            <p><strong>Khách hàng:</strong> ${escapeHtml(order.tenNguoiDung || 'N/A')}</p>
            <p><strong>Địa chỉ giao hàng:</strong> ${escapeHtml(order.diaChiGiaoHang || 'N/A')}</p>
            <p><strong>Trạng thái:</strong> ${escapeHtml(order.trangThai)}</p>
            <p><strong>Tổng tiền:</strong> ${formatCurrency(order.tongTien)}</p>
            <hr>
            <h6>Chi tiết sản phẩm:</h6>
            <ul>
                ${order.chiTiet && order.chiTiet.length > 0
                    ? order.chiTiet.map(ct => `
                        <li>
                            ${escapeHtml(ct.sanPham || '')} 
                            (${escapeHtml(ct.kichThuoc || '')}) 
                            - SL: ${ct.soLuong}, 
                            Giá: ${formatCurrency(ct.gia)}
                        </li>
                    `).join('')
                    : '<li>Không có sản phẩm.</li>'
                }
            </ul>
        `;

        $('#orderDetailModal').modal('show');
    }

    // =========================
    // ✏️ Hiển thị modal cập nhật trạng thái
    // =========================
    function showUpdateModal(id) {
        document.getElementById('updateOrderId').value = id;
        document.getElementById('newStatus').value = ""; // reset
        $('#statusModal').modal('show');
    }    

    // =========================
    // ✅ Cập nhật trạng thái đơn hàng
    // =========================
    async function updateStatus() {
        const id = document.getElementById('updateOrderId').value;
    
        // Không trim để giữ đúng “đã vận chuyển ”
        const newStatus = document.getElementById('newStatus').value;
    
        if (!newStatus) return showToast('Vui lòng chọn trạng thái mới!', 'warning');
    
        try {
            const res = await fetchJson(`${baseUrl}/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newStatus)
            });
    
            showToast(res.message || 'Cập nhật trạng thái thành công!', 'success');
    
            // ✅ Đúng với Bootstrap 4
            $('#statusModal').modal('hide');
    
            // ✅ Bỏ cache khi reload
            await loadOrders();
    
        } catch (err) {
            console.error('Update status failed', err);
            showToast('Lỗi khi cập nhật trạng thái: ' + err.message, 'danger');
        }
    }
    

    // =========================
    // 🎨 Hàm tiện ích lấy lớp màu cho Badge
    // =========================
    function getStatusBadgeClass(status) {
        // Luôn chuyển sang chữ thường và loại bỏ dấu cách thừa (như "đã vận chuyển ")
        const normalizedStatus = status ? status.trim().toLowerCase() : ''; 
        
        switch (normalizedStatus) {
            case 'đã giao':
                return 'badge-success'; // Màu xanh lá (Thành công)
            case 'đang xử lý':
                return 'badge-primary'; // Màu xanh dương (Thông tin)
            case 'đã vận chuyển':
                return 'badge-warning'; // Màu vàng (Cảnh báo)
            case 'đã hủy':
                return 'badge-danger'; // Màu đỏ (Nguy hiểm)
            default:
                return 'badge-secondary'; // Màu xám (Mặc định)
        }
    }

    function showToast(message, type = "success") {
        const toastContainer = document.getElementById("toastContainer");
    
        const toast = document.createElement("div");
        // Chuyển đổi type sang class tương thích BS4
        let bgClass = "";
        switch (type) {
            case "success": bgClass = "bg-success text-white"; break;
            case "danger": bgClass = "bg-danger text-white"; break;
            case "warning": bgClass = "bg-warning text-dark"; break;
            default: bgClass = "bg-secondary text-white";
        }
    
        toast.className = `toast align-items-center ${bgClass} border-0 show mb-2`;
        toast.role = "alert";
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
            </div>`;
    
        toastContainer.appendChild(toast);
    
        // Tự động ẩn sau 3.5 giây
        setTimeout(() => toast.remove(), 3500);
    }    

    // =========================
    // ⚡ Khởi động trang
    // =========================
    document.addEventListener('DOMContentLoaded', () => {
        // Nút tìm kiếm
        const searchBtn = document.getElementById('searchBtn');
        if (searchBtn) searchBtn.addEventListener('click', searchOrder);

        // Nút lưu trạng thái
        const saveStatusBtn = document.getElementById('saveStatusBtn');
        if (saveStatusBtn) saveStatusBtn.addEventListener('click', updateStatus);

        // Tải danh sách ban đầu
        loadOrders();
    });