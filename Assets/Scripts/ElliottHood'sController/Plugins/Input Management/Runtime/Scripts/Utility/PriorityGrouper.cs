using System.Collections.Generic;
using System.Linq;

namespace InputManagement
{
    [System.Serializable]
    public class PriorityGrouper<T> where T : IPriorityGroupable
    {
        public List<T> Items = new List<T>();

        public void AddItem(T item)
        {
            if (Items.Contains(item))
                return;
            Items.Add(item);
        }

        public bool RemoveItem(T item)
        {
            return Items.Remove(item);
        }
        
        /// <returns>All retrievable items with the highest priority among retrievable items, and their priority</returns>
        public List<T> GetHighestPriorityItems(out int requiredPriority)
        {
            requiredPriority = 0;
            RemoveNullItems();

            // Get the items that are retrievable
            List<T> retrievableItems = Items.Where(item => item.GetRetrievable()).ToList();

            if (retrievableItems.Count == 0)
                return new List<T>();

            // Get the maximum priority of retrievable items
            int maxPriority = retrievableItems.Max(item => item.GetPriority());
            requiredPriority = maxPriority;

            // Return all retrievable items with the highest priority
            return retrievableItems.Where(item => item.GetPriority() == maxPriority).ToList();
        }

        private void RemoveNullItems()
        {
            Items = Items.Where(item => item != null).ToList();
        }
    }

    public interface IPriorityGroupable
    {
        int GetPriority();
        bool GetRetrievable();
    }
}