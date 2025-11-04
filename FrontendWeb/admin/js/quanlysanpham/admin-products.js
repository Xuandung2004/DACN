// Base API URL - adjust port/host if your API runs elsewhere
const baseUrl = 'http://localhost:5150/api/SanPham';
// Debug: print the baseUrl so you can see in browser console which URL the page will call
console.log('[admin-products] baseUrl =', baseUrl);

async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    if (!res.ok) {
        const text = await res.text();
        throw new Error(`${res.status} ${res.statusText} - ${text}`);
    }
    return res.json();
}

async function loadProducts() {
    try {
        const products = await fetchJson(baseUrl);
        const tbody = document.getElementById('productsBody');
        tbody.innerHTML = '';
        products.forEach(p => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${p.id}</td>
                <td>${escapeHtml(p.tenSp || '')}</td>
                <td>${p.gia ?? ''}</td>
                <td>${p.danhMucId ?? ''}</td>
                <td>${p.tonKho ?? ''}</td>
                <td>
                    <button class="btn btn-sm btn-info" onclick="editProduct(${p.id})">Sửa</button>
                    <button class="btn btn-sm btn-danger" onclick="deleteProduct(${p.id})">Xóa</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
        // init DataTable if desired
        if (window.jQuery && typeof jQuery.fn.DataTable === 'function') {
            if (!$.fn.dataTable.isDataTable('#productsTable')) {
                $('#productsTable').DataTable();
            }
        }
    } catch (err) {
        console.error('Load products failed', err);
        alert('Không thể tải danh sách sản phẩm: ' + err.message);
    }
}

function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function openModalForCreate() {
    document.getElementById('modalTitle').innerText = 'Thêm sản phẩm';
    document.getElementById('productForm').reset();
    document.getElementById('Id').value = '';
    $('#productModal').modal('show');
}

async function editProduct(id) {
    try {
        const p = await fetchJson(`${baseUrl}/${id}`);
        document.getElementById('modalTitle').innerText = 'Sửa sản phẩm';
        document.getElementById('Id').value = p.id;
        document.getElementById('TenSp').value = p.tenSp || '';
        document.getElementById('MoTa').value = p.moTa || '';
        document.getElementById('Gia').value = p.gia ?? '';
        document.getElementById('TonKho').value = p.tonKho ?? '';
        document.getElementById('DanhMucId').value = p.danhMucId ?? '';
        document.getElementById('AnhId').value = p.anhId ?? '';
        $('#productModal').modal('show');
    } catch (err) {
        console.error(err);
        alert('Không lấy được sản phẩm: ' + err.message);
    }
}

async function saveProduct() {
    const id = document.getElementById('Id').value;
    const payload = {
        id: id ? Number(id) : 0,
        tenSp: document.getElementById('TenSp').value.trim(),
        moTa: document.getElementById('MoTa').value.trim(),
        gia: Number(document.getElementById('Gia').value) || 0,
        tonKho: Number(document.getElementById('TonKho').value) || 0,
        danhMucId: Number(document.getElementById('DanhMucId').value) || null,
        anhId: Number(document.getElementById('AnhId').value) || null
    };

    try {
        if (!payload.tenSp) { alert('Tên sản phẩm là bắt buộc'); return; }

        if (id) {
            // update
            const res = await fetch(`${baseUrl}/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
        } else {
            // create
            const res = await fetch(baseUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) {
                const text = await res.text();
                throw new Error(`${res.status} ${res.statusText} - ${text}`);
            }
        }
        $('#productModal').modal('hide');
        await loadProducts();
    } catch (err) {
        console.error('Save product failed', err);
        alert('Lưu thất bại: ' + err.message);
    }
}

async function deleteProduct(id) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) return;
    try {
        const res = await fetch(`${baseUrl}/${id}`, { method: 'DELETE' });
        if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
        await loadProducts();
    } catch (err) {
        console.error('Delete failed', err);
        alert('Xóa thất bại: ' + err.message);
    }
}

// wire events
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('btnNew').addEventListener('click', openModalForCreate);
    document.getElementById('saveBtn').addEventListener('click', saveProduct);
    loadProducts();
});
