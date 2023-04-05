using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    internal class TrackedObject
    {
        public object Entity { get; set; }
        public MetaTableBD MetaTable { get; set; }
        public TrackingState TrackingState { get; set; }
    }
    enum TrackingState
    {
        ToBeInserted,
        ToBeUpdated,
        ToBeDeleted,

    }
}
