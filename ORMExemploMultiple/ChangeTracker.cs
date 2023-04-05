using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    public class ChangeTracker
    {
        List<WeakReference> _trackedItems;

        internal void AcceptChanges() 
        {
        
        }
        void Attach()
        {

        }
        void Detach()
        {

        }
        void GetInsertingObjects()
        {

        }
        void GetTrackedObjectByKey()
        {

        }
        void RegisterDelete()
        {

        }
        void RegisterInsert()
        {

        }
        private Dictionary<object, TrackedObject> _items;
        private void OnPropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            // Is this object already being tracked?
            //TrackedObject trackedObj;
            //if (!_items.TryGetValue(sender, out trackedObj))
            //{
            //    MetaTableBD metaTable = CheckAndGetMetaTable(sender.GetType());
            //    // This has not been tracked in the past so create a new tracked object
            //    trackedObj = ...;
            //    _items.Add(sender, trackedObj);
            //}
        }

        internal IEnumerable<TrackedObject> GetInterestingObjects()
        {
            throw new NotImplementedException();
        }
    }
}
