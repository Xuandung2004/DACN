// Base API URLs
const baseUrl = 'http://localhost:5150/api/SanPham';
const danhMucUrl = 'http://localhost:5150/api/DanhMuc';

// Debug: print the baseUrl so you can see in browser console
console.log('[admin-products] baseUrl =', baseUrl);

// Load danh mục vào select box
async function loadDanhMuc() {
    try {
        const danhmucs = await fetchJson(danhMucUrl);
        const select = document.getElementById('DanhMucId');

        // Clear current options except first one
        const firstOption = select.firstChild;
        select.innerHTML = '';
        select.appendChild(firstOption);

        // Add new options
        danhmucs.forEach(dm => {
            const option = document.createElement('option');
            option.value = dm.id;
            option.textContent = dm.tenDanhMuc;
            select.appendChild(option);
        });
    } catch (err) {
        console.error('Load danh mục failed', err);
        alert('Không thể tải danh sách danh mục: ' + err.message);
    }
}

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
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#productsTable')) {
            $('#productsTable').DataTable().destroy();
        }

        const products = await fetchJson(baseUrl);
        const tbody = document.getElementById('productsBody');
        tbody.innerHTML = '';

        products.forEach(p => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${p.id}</td>
                <td>${escapeHtml(p.tenSp || '')}</td>
                <td>${formatCurrency(p.gia)}</td>
                <td>${escapeHtml(p.danhMuc?.tenDanhMuc || '')}</td>
                <td>${p.tonKho ?? ''}</td>
                <td>
                    <button class="btn btn-sm btn-info" onclick="editProduct(${p.id})">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteProduct(${p.id})">
                        <i class="fas fa-trash"></i> Xóa
                    </button>
                </td>
            `;
            tbody.appendChild(tr);
        });

        // Reinitialize DataTable with Vietnamese language
        $('#productsTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json'
            }
        });
    } catch (err) {
        console.error('Load products failed', err);
        alert('Không thể tải danh sách sản phẩm: ' + err.message);
    }
}

// Format currency
function formatCurrency(value) {
    if (!value) return '';
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(value);
}

// HTML Escape function
function escapeHtml(str) {
    if (!str) return '';
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
        document.getElementById('DanhMucId').value = p.danhMuc?.id ?? '';
        document.getElementById('Slug').value = p.slug || '';

        // Set image preview if available
        const imgPreview = document.getElementById('imagePreview');
        if (imgPreview && p.anhs && p.anhs.length > 0) {
            imgPreview.src = p.anhs[0].duongDan;
            imgPreview.style.display = 'block';
        } else if (imgPreview) {
            imgPreview.style.display = 'none';
        }

        $('#productModal').modal('show');
    } catch (err) {
        console.error('Load product failed', err);
        alert('Không thể tải thông tin sản phẩm: ' + err.message);
    }
}

async function saveProduct() {
    const form = document.getElementById('productForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const id = document.getElementById('Id').value;
    const payload = {
        id: id ? Number(id) : 0,
        tenSp: document.getElementById('TenSp').value.trim(),
        moTa: document.getElementById('MoTa').value.trim(),
        gia: Number(document.getElementById('Gia').value) || 0,
        tonKho: Number(document.getElementById('TonKho').value) || 0,
        danhMucId: Number(document.getElementById('DanhMucId').value) || null,
        slug: document.getElementById('Slug').value.trim(),
    };

    // Handle file upload if available
    const fileInput = document.getElementById('imageInput');
    if (fileInput && fileInput.files.length > 0) {
        const formData = new FormData();
        formData.append('image', fileInput.files[0]);

        try {
            const uploadRes = await fetch(`${baseUrl}/upload-image`, {
                method: 'POST',
                body: formData
            });

            if (!uploadRes.ok) {
                throw new Error('Tải ảnh lên thất bại');
            }

            const imageData = await uploadRes.json();
            payload.anhId = imageData.id;
        } catch (err) {
            console.error('Image upload failed', err);
            alert('Tải ảnh lên thất bại: ' + err.message);
            return;
        }
    }

    try {
        if (!payload.tenSp) {
            alert('Vui lòng nhập tên sản phẩm');
            return;
        }

        let res;
        if (id) {
            res = await fetch(`${baseUrl}/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
        } else {
            res = await fetch(baseUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
        }

        if (!res.ok) {
            const errText = await res.text();
            throw new Error(`${res.status} ${res.statusText} - ${errText}`);
        }

        $('#productModal').modal('hide');
        await loadProducts();

        // Show success message
        alert(id ? 'Cập nhật thành công!' : 'Thêm mới thành công!');
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

// Show image preview when file is selected
function setupImagePreview() {
    const input = document.getElementById('imageInput');
    const preview = document.getElementById('imagePreview');

    if (input && preview) {
        input.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                };
                reader.readAsDataURL(this.files[0]);
            } else {
                preview.style.display = 'none';
            }
        });
    }
}

// wire events
document.addEventListener('DOMContentLoaded', () => {
    // Handle modal reset when showing
    $('#productModal').on('show.bs.modal', function (e) {
        // If triggered by "Thêm mới" link
        if (e.relatedTarget && e.relatedTarget.innerText === 'Thêm mới') {
            openModalForCreate();
        }

        // Load danh mục if not loaded
        if (!document.querySelector('#DanhMucId option:not(:first-child)')) {
            loadDanhMuc();
        }
    });

    // Setup image preview
    setupImagePreview();

    // Handle save button click
    const saveBtn = document.getElementById('saveBtn');
    if (saveBtn) {
        saveBtn.addEventListener('click', saveProduct);
    }

    // Load initial data
    loadProducts();
});
