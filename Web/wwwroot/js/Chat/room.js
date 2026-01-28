(function () {
'use strict';

const roomId = parseInt(document.getElementById('roomId')?.value || '0');
const currentUserId = parseInt(document.getElementById('currentUserId')?.value || '0');
const messagesContainer = document.getElementById('messagesContainer');
const messagesList = document.getElementById('messagesList');
const messageForm = document.getElementById('messageForm');
const messageInput = document.getElementById('messageInput');
const editModal = document.getElementById('editModal');
const editMessageInput = document.getElementById('editMessageInput');
const editMessageId = document.getElementById('editMessageId');

let connection = null;

// Convert UTC time to Vietnam timezone (UTC+7)
function toVietnamTime(utcDateString) {
    const date = new Date(utcDateString);
    // Add 7 hours for Vietnam timezone
    const vietnamOffset = 7 * 60; // minutes
    const utcOffset = date.getTimezoneOffset(); // local offset in minutes (negative for ahead of UTC)
    const vietnamTime = new Date(date.getTime() + (vietnamOffset + utcOffset) * 60 * 1000);
    return vietnamTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', hour12: false });
}

// Initialize SignalR connection
async function initSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/chat')
        .withAutomaticReconnect()
            .build();

        connection.on('message:new', function (message) {
            appendMessage(message);
            scrollToBottom();
        });

        connection.on('message:edited', function (data) {
            updateMessageContent(data.messageId, data.newContent);
        });

        connection.on('message:deleted', function (data) {
            markMessageDeleted(data.messageId);
        });

        connection.on('error', function (errorMessage) {
            showToast(errorMessage, 'error');
        });

        connection.on('joined', function (joinedRoomId) {
            console.log('Joined room:', joinedRoomId);
        });

        try {
            await connection.start();
            console.log('SignalR connected');
            await connection.invoke('JoinRoom', roomId);
        } catch (err) {
            console.error('SignalR connection error:', err);
            setTimeout(initSignalR, 5000);
        }
    }

    // Append new message to the list
    function appendMessage(message) {
        const isOwn = message.senderId === currentUserId;
        const isSystem = message.messageType === 'SYSTEM';
        const isDeleted = message.deletedAt != null;

        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${isOwn ? 'own' : ''} ${isSystem ? 'system' : ''} ${isDeleted ? 'deleted' : ''}`;
        messageDiv.setAttribute('data-message-id', message.messageId);
        messageDiv.setAttribute('data-sender-id', message.senderId);

        if (isSystem) {
            messageDiv.innerHTML = `
                <div class="message-system-content">
                    <i class="fas fa-info-circle"></i>
                    ${escapeHtml(message.content)}
                </div>
            `;
        } else if (isDeleted) {
            messageDiv.innerHTML = `
                <div class="message-deleted-content">
                    <i class="fas fa-ban"></i>
                    <em>This message has been deleted</em>
                </div>
            `;
        } else {
            const time = toVietnamTime(message.createdAt);
            const editedTag = message.editedAt ? '<span class="message-edited">(edited)</span>' : '';
            const actions = isOwn ? `
                <div class="message-actions">
                    <button type="button" class="action-btn edit-btn" onclick="startEdit(${message.messageId})">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button type="button" class="action-btn delete-btn" onclick="deleteMessage(${message.messageId})">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            ` : '';

            messageDiv.innerHTML = `
                ${actions}
                <div class="message-bubble">
                    <div class="message-content">${escapeHtml(message.content)}</div>
                    <div class="message-meta">
                        <span class="message-time">${time}</span>
                        ${editedTag}
                    </div>
                </div>
            `;
        }

        messagesList.appendChild(messageDiv);
    }

    // Update message content after edit
    function updateMessageContent(messageId, newContent) {
        const messageEl = document.querySelector(`[data-message-id="${messageId}"]`);
        if (!messageEl) return;

        const contentEl = messageEl.querySelector('.message-content');
        if (contentEl) {
            contentEl.textContent = newContent;
        }

        const metaEl = messageEl.querySelector('.message-meta');
        if (metaEl && !metaEl.querySelector('.message-edited')) {
            const editedSpan = document.createElement('span');
            editedSpan.className = 'message-edited';
            editedSpan.textContent = '(edited)';
            metaEl.appendChild(editedSpan);
        }
    }

    // Mark message as deleted
    function markMessageDeleted(messageId) {
        const messageEl = document.querySelector(`[data-message-id="${messageId}"]`);
        if (!messageEl) return;

        messageEl.classList.add('deleted');
        const bubble = messageEl.querySelector('.message-bubble');
        if (bubble) {
            bubble.innerHTML = `
                <div class="message-deleted-content">
                    <i class="fas fa-ban"></i>
                    <em>This message has been deleted</em>
                </div>
            `;
        }
    }

    // Scroll to bottom of messages
    function scrollToBottom() {
        if (messagesContainer) {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }

    // Escape HTML to prevent XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text || '';
        return div.innerHTML;
    }

    // Show toast notification
    function showToast(message, type) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('show');
        }, 100);

        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    // Get anti-forgery token
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // Send message via SignalR
    async function sendMessage(content) {
        if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
            showToast('Connection lost. Reconnecting...', 'error');
            return;
        }

        try {
            await connection.invoke('SendMessage', roomId, content);
        } catch (err) {
            console.error('Send message error:', err);
            showToast('Failed to send message', 'error');
        }
    }

    // Start editing a message
    window.startEdit = function (messageId) {
        const messageEl = document.querySelector(`[data-message-id="${messageId}"]`);
        if (!messageEl) return;

        const contentEl = messageEl.querySelector('.message-content');
        if (!contentEl) return;

        editMessageId.value = messageId;
        editMessageInput.value = contentEl.textContent;
        editModal.classList.add('open');
        editMessageInput.focus();
    };

    // Close edit modal
    window.closeEditModal = function () {
        editModal.classList.remove('open');
        editMessageId.value = '';
        editMessageInput.value = '';
    };

    // Confirm edit
    window.confirmEdit = async function () {
        const messageId = parseInt(editMessageId.value);
        const newContent = editMessageInput.value.trim();

        if (!newContent) {
            showToast('Message cannot be empty', 'error');
            return;
        }

        try {
            await connection.invoke('EditMessage', roomId, messageId, newContent);
            closeEditModal();
        } catch (err) {
            console.error('Edit message error:', err);
            showToast('Failed to edit message', 'error');
        }
    };

    // Delete message
    window.deleteMessage = async function (messageId) {
        if (!confirm('Are you sure you want to delete this message?')) {
            return;
        }

        try {
            await connection.invoke('DeleteMessage', roomId, messageId);
        } catch (err) {
            console.error('Delete message error:', err);
            showToast('Failed to delete message', 'error');
        }
    };

    // Handle form submission
    if (messageForm) {
        messageForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const content = messageInput.value.trim();
            if (!content) return;

            messageInput.value = '';
            await sendMessage(content);
        });
    }

    // Auto-resize textarea
    if (messageInput) {
        messageInput.addEventListener('input', function () {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 120) + 'px';
        });

        messageInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                messageForm.dispatchEvent(new Event('submit'));
            }
        });
    }

    // Initialize on page load
    scrollToBottom();
    initSignalR();

    // Cleanup on page unload
    window.addEventListener('beforeunload', async function () {
        if (connection) {
            try {
                await connection.invoke('LeaveRoom', roomId);
                await connection.stop();
            } catch (err) {
                console.error('Cleanup error:', err);
            }
        }
    });
})();
