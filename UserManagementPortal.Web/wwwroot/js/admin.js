document.addEventListener('DOMContentLoaded', () => {
    const selectAll = document.getElementById('selectAll');
    const rowChecks = () => Array.from(document.querySelectorAll('.row-check'));
    const toolbarButtons = ['btnBlock', 'btnUnblock', 'btnDelete', 'btnDeleteUnverified']
        .map(id => document.getElementById(id));
    const statusEl = document.getElementById('statusMessage');
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    function getSelectedIds() {
        return rowChecks().filter(c => c.checked).map(c => Number(c.value));
    }

    function refreshToolbarState() {
        const anySelected = getSelectedIds().length > 0;
        toolbarButtons.forEach(btn => btn.disabled = !anySelected);

        const boxes = rowChecks();
        const checkedCount = boxes.filter(c => c.checked).length;
        selectAll.checked = boxes.length > 0 && checkedCount === boxes.length;
        selectAll.indeterminate = checkedCount > 0 && checkedCount < boxes.length;
    }

    selectAll.addEventListener('change', () => {
        rowChecks().forEach(c => c.checked = selectAll.checked);
        refreshToolbarState();
    });

    document.getElementById('usersTable').addEventListener('change', (e) => {
        if (e.target.classList.contains('row-check')) refreshToolbarState();
    });

    function showStatus(message, isError) {
        statusEl.textContent = message;
        statusEl.className = `alert ${isError ? 'alert-danger' : 'alert-success'}`;
    }

    async function postAction(url, ids) {
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ ids })
        });

        let data;
        try { data = await res.json(); } catch { data = null; }

        if (!res.ok || !data?.success) {
            showStatus(data?.message || 'Operation failed. Please try again.', true);
            return;
        }

        showStatus(`Done: ${data.affectedCount} user(s) affected.`, false);

        if (data.redirectToLogin) {
            window.location.href = '/Account/Login';
            return;
        }

        window.location.reload();
    }

    document.getElementById('btnBlock').addEventListener('click', () =>
        postAction('/Admin/Block', getSelectedIds()));
    document.getElementById('btnUnblock').addEventListener('click', () =>
        postAction('/Admin/Unblock', getSelectedIds()));
    document.getElementById('btnDelete').addEventListener('click', () =>
        postAction('/Admin/Delete', getSelectedIds()));
    document.getElementById('btnDeleteUnverified').addEventListener('click', () =>
        postAction('/Admin/DeleteUnverified', getSelectedIds()));

    document.getElementById('filterInput').addEventListener('input', (e) => {
        const q = e.target.value.trim().toLowerCase();
        document.querySelectorAll('#usersTable tbody tr').forEach(row => {
            const match = row.dataset.name.includes(q) || row.dataset.email.includes(q);
            row.classList.toggle('d-none', !match);
        });
    });

    document.querySelectorAll('[data-bs-toggle="tooltip"]')
        .forEach(el => new bootstrap.Tooltip(el));
});