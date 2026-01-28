(function () {
    'use strict';

    // DOM Elements
    const modal = document.getElementById('createChatModal');
    const btnOpenModal = document.getElementById('btnOpenCreateModal');
    const btnCreateFirst = document.getElementById('btnCreateFirst');
    const btnCloseModal = document.getElementById('btnCloseModal');
    const btnCancelCreate = document.getElementById('btnCancelCreate');
    const btnConfirmCreate = document.getElementById('btnConfirmCreate');

    const tabButtons = document.querySelectorAll('.tab-btn');
    const dmTabContent = document.getElementById('dmTabContent');
    const groupTabContent = document.getElementById('groupTabContent');

    const dmUserSearch = document.getElementById('dmUserSearch');
    const dmUsersList = document.getElementById('dmUsersList');
    const groupUserSearch = document.getElementById('groupUserSearch');
    const groupUsersList = document.getElementById('groupUsersList');
    const groupNameInput = document.getElementById('groupName');
    const selectedMembersContainer = document.getElementById('selectedMembers');

    // State
    let currentTab = 'dm';
    let selectedDmUserId = null;
    let selectedGroupMembers = [];
    let searchTimeout = null;

    // Initialize
    function init() {
        if (btnOpenModal) {
            btnOpenModal.addEventListener('click', openModal);
        }
        if (btnCreateFirst) {
            btnCreateFirst.addEventListener('click', openModal);
        }
        if (btnCloseModal) {
            btnCloseModal.addEventListener('click', closeModal);
        }
        if (btnCancelCreate) {
            btnCancelCreate.addEventListener('click', closeModal);
        }
        if (btnConfirmCreate) {
            btnConfirmCreate.addEventListener('click', handleCreate);
        }

        tabButtons.forEach(btn => {
            btn.addEventListener('click', () => switchTab(btn.dataset.tab));
        });

        if (dmUserSearch) {
            dmUserSearch.addEventListener('input', () => debounceSearch(dmUserSearch.value, 'dm'));
        }
        if (groupUserSearch) {
            groupUserSearch.addEventListener('input', () => debounceSearch(groupUserSearch.value, 'group'));
        }
        if (groupNameInput) {
            groupNameInput.addEventListener('input', updateConfirmButton);
        }

        // Close modal on overlay click
        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    closeModal();
                }
            });
        }

        // Close on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && modal.classList.contains('open')) {
                closeModal();
            }
        });
    }

    function openModal() {
        if (modal) {
            modal.classList.add('open');
            resetModal();
        }
    }

    function closeModal() {
        if (modal) {
            modal.classList.remove('open');
        }
    }

    function resetModal() {
        currentTab = 'dm';
        selectedDmUserId = null;
        selectedGroupMembers = [];

        switchTab('dm');

        if (dmUserSearch) dmUserSearch.value = '';
        if (groupUserSearch) groupUserSearch.value = '';
        if (groupNameInput) groupNameInput.value = '';

        renderDmUsersList([]);
        renderGroupUsersList([]);
        renderSelectedMembers();
        updateConfirmButton();
    }

    function switchTab(tab) {
        currentTab = tab;

        tabButtons.forEach(btn => {
            btn.classList.toggle('active', btn.dataset.tab === tab);
        });

        if (dmTabContent) dmTabContent.classList.toggle('active', tab === 'dm');
        if (groupTabContent) groupTabContent.classList.toggle('active', tab === 'group');

        updateConfirmButton();
    }

    function debounceSearch(query, type) {
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }
        searchTimeout = setTimeout(() => searchUsers(query, type), 300);
    }

    async function searchUsers(query, type) {
        const listContainer = type === 'dm' ? dmUsersList : groupUsersList;

        if (!query || query.length < 2) {
            renderPlaceholder(listContainer, type);
            return;
        }

        listContainer.innerHTML = '<div class="users-loading"><i class="fas fa-spinner"></i> Searching...</div>';

        try {
            const response = await fetch(`/Chat/AvailableUsers?search=${encodeURIComponent(query)}`);
            if (!response.ok) throw new Error('Failed to search users');

            const users = await response.json();

            if (type === 'dm') {
                renderDmUsersList(users);
            } else {
                renderGroupUsersList(users);
            }
        } catch (error) {
            console.error('Search error:', error);
            listContainer.innerHTML = '<div class="users-placeholder"><i class="fas fa-exclamation-circle"></i><p>Error searching users</p></div>';
        }
    }

    function renderPlaceholder(container, type) {
        const icon = type === 'dm' ? 'fa-search' : 'fa-user-plus';
        const text = type === 'dm' ? 'Search for users to start a conversation' : 'Search for users to add to your group';
        container.innerHTML = `<div class="users-placeholder"><i class="fas ${icon}"></i><p>${text}</p></div>`;
    }

    function renderDmUsersList(users) {
        if (!users || users.length === 0) {
            if (dmUserSearch && dmUserSearch.value.length >= 2) {
                dmUsersList.innerHTML = '<div class="users-placeholder"><i class="fas fa-user-slash"></i><p>No users found</p></div>';
            } else {
                renderPlaceholder(dmUsersList, 'dm');
            }
            return;
        }

        dmUsersList.innerHTML = users.map(user => `
            <div class="user-item ${selectedDmUserId === user.userId ? 'selected' : ''}" data-user-id="${user.userId}">
                <div class="user-avatar">${getInitials(user.fullName)}</div>
                <div class="user-details">
                    <div class="user-name">${escapeHtml(user.fullName)}</div>
                    <div class="user-role">${user.role.toLowerCase()}</div>
                </div>
                ${selectedDmUserId === user.userId ? '<i class="fas fa-check-circle user-check"></i>' : ''}
            </div>
        `).join('');

        dmUsersList.querySelectorAll('.user-item').forEach(item => {
            item.addEventListener('click', () => selectDmUser(parseInt(item.dataset.userId)));
        });
    }

    function renderGroupUsersList(users) {
        if (!users || users.length === 0) {
            if (groupUserSearch && groupUserSearch.value.length >= 2) {
                groupUsersList.innerHTML = '<div class="users-placeholder"><i class="fas fa-user-slash"></i><p>No users found</p></div>';
            } else {
                renderPlaceholder(groupUsersList, 'group');
            }
            return;
        }

        const filteredUsers = users.filter(u => !selectedGroupMembers.some(m => m.userId === u.userId));

        if (filteredUsers.length === 0) {
            groupUsersList.innerHTML = '<div class="users-placeholder"><i class="fas fa-users"></i><p>All matching users already added</p></div>';
            return;
        }

        groupUsersList.innerHTML = filteredUsers.map(user => `
            <div class="user-item" data-user-id="${user.userId}" data-user-name="${escapeHtml(user.fullName)}">
                <div class="user-avatar">${getInitials(user.fullName)}</div>
                <div class="user-details">
                    <div class="user-name">${escapeHtml(user.fullName)}</div>
                    <div class="user-role">${user.role.toLowerCase()}</div>
                </div>
                <i class="fas fa-plus user-check" style="color: #94a3b8;"></i>
            </div>
        `).join('');

        groupUsersList.querySelectorAll('.user-item').forEach(item => {
            item.addEventListener('click', () => {
                addGroupMember({
                    userId: parseInt(item.dataset.userId),
                    fullName: item.dataset.userName
                });
            });
        });
    }

    function selectDmUser(userId) {
        selectedDmUserId = userId;
        // Re-render to show selection
        const items = dmUsersList.querySelectorAll('.user-item');
        items.forEach(item => {
            const isSelected = parseInt(item.dataset.userId) === userId;
            item.classList.toggle('selected', isSelected);
            const checkIcon = item.querySelector('.user-check');
            if (isSelected && !checkIcon) {
                const icon = document.createElement('i');
                icon.className = 'fas fa-check-circle user-check';
                item.appendChild(icon);
            } else if (!isSelected && checkIcon) {
                checkIcon.remove();
            }
        });
        updateConfirmButton();
    }

    function addGroupMember(user) {
        if (selectedGroupMembers.some(m => m.userId === user.userId)) {
            return;
        }
        selectedGroupMembers.push(user);
        renderSelectedMembers();
        // Re-render group users list to hide added user
        if (groupUserSearch && groupUserSearch.value.length >= 2) {
            searchUsers(groupUserSearch.value, 'group');
        }
        updateConfirmButton();
    }

    function removeGroupMember(userId) {
        selectedGroupMembers = selectedGroupMembers.filter(m => m.userId !== userId);
        renderSelectedMembers();
        // Re-render group users list
        if (groupUserSearch && groupUserSearch.value.length >= 2) {
            searchUsers(groupUserSearch.value, 'group');
        }
        updateConfirmButton();
    }

    function renderSelectedMembers() {
        if (!selectedMembersContainer) return;

        if (selectedGroupMembers.length === 0) {
            selectedMembersContainer.innerHTML = '';
            return;
        }

        selectedMembersContainer.innerHTML = selectedGroupMembers.map(member => `
            <div class="member-chip">
                <span>${escapeHtml(member.fullName)}</span>
                <button type="button" data-user-id="${member.userId}" title="Remove">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `).join('');

        selectedMembersContainer.querySelectorAll('.member-chip button').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                removeGroupMember(parseInt(btn.dataset.userId));
            });
        });
    }

    function updateConfirmButton() {
        if (!btnConfirmCreate) return;

        let isValid = false;

        if (currentTab === 'dm') {
            isValid = selectedDmUserId !== null;
        } else {
            const groupName = groupNameInput ? groupNameInput.value.trim() : '';
            isValid = groupName.length > 0;
        }

        btnConfirmCreate.disabled = !isValid;
    }

    async function handleCreate() {
        if (btnConfirmCreate.disabled) return;

        btnConfirmCreate.disabled = true;
        btnConfirmCreate.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating...';

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            if (currentTab === 'dm') {
                await createDm(token);
            } else {
                await createGroup(token);
            }
        } catch (error) {
            console.error('Create error:', error);
            showToast(error.message || 'Failed to create chat', 'error');
            btnConfirmCreate.disabled = false;
            btnConfirmCreate.innerHTML = '<i class="fas fa-paper-plane"></i> Create Chat';
        }
    }

    async function createDm(token) {
        const response = await fetch('/Chat/CreateDm', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ otherUserId: selectedDmUserId })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.error || 'Failed to create DM');
        }

        const room = await response.json();
        window.location.href = `/Chat/Room/${room.roomId}`;
    }

    async function createGroup(token) {
        const groupName = groupNameInput ? groupNameInput.value.trim() : '';
        const memberIds = selectedGroupMembers.map(m => m.userId);

        const response = await fetch('/Chat/CreateGroup', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                roomName: groupName,
                memberUserIds: memberIds
            })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.error || 'Failed to create group');
        }

        const room = await response.json();
        window.location.href = `/Chat/Room/${room.roomId}`;
    }

    function getInitials(name) {
        if (!name) return '?';
        const parts = name.trim().split(' ');
        if (parts.length === 1) {
            return parts[0].charAt(0).toUpperCase();
        }
        return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text || '';
        return div.innerHTML;
    }

    function showToast(message, type) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        toast.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            padding: 12px 20px;
            background: ${type === 'error' ? '#ef4444' : '#22c55e'};
            color: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 2000;
            opacity: 0;
            transform: translateY(10px);
            transition: all 0.3s ease;
        `;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateY(0)';
        }, 100);

        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(10px)';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
