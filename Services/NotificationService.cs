using System;
using System.Collections.Generic;
using System.Linq;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Owns notification storage, read status, and notification events for reactive UI updates.
    /// </summary>
    public class NotificationService
    {
        public event Action<Notification>? OnNewNotification;
        private readonly List<Notification> _all = new();

        public void AddNotification(Notification n)
        {
            if (n.Id == Guid.Empty) n.Id = Guid.NewGuid();
            if (n.Timestamp == default) n.Timestamp = DateTime.Now;
            _all.Add(n);
            OnNewNotification?.Invoke(n);
        }

        public void AddNotification(string message, NotificationType type, Guid userId) => AddNotification(new Notification { Message = message, NotificationType = type, UserId = userId, Timestamp = DateTime.Now });
        public List<Notification> GetForUser(Guid userId) => _all.Where(n => n.UserId == userId).OrderByDescending(n => n.Timestamp).ToList();
        public int GetUnreadCount(Guid userId) => _all.Count(n => n.UserId == userId && !n.IsRead);
        public void MarkAsRead(Guid notificationId) { var n = _all.FirstOrDefault(x => x.Id == notificationId); if (n != null) n.IsRead = true; }
        public void MarkAllAsRead(Guid userId) { foreach (var n in _all.Where(x => x.UserId == userId)) n.IsRead = true; }
    }
}
