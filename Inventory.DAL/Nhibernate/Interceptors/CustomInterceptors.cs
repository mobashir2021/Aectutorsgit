using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Helpers;
using NHibernate;
using NHibernate.Type;

namespace AECMIS.DAL.Nhibernate.Interceptors
{
    public class CustomInterceptor : EmptyInterceptor
    {
        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
            if (entity is Entity)
            {
                for (var i = 0; i < propertyNames.Length; i++)
                {
                    if ("ModifiedDate".Equals(propertyNames[i]))
                    {
                        currentState[i] = DateTime.UtcNow;
                    }

                    if ("ModifiedBy".Equals(propertyNames[i]))
                    {
                        currentState[i] = Users.GetCurrentUser();
                    }
                }

                return true;
            }
            return false;
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            if (entity is Entity)
            {
                for (var i = 0; i < propertyNames.Length; i++)
                {
                    if ("CreatedDate".Equals(propertyNames[i]) || "ModifiedDate".Equals(propertyNames[i]))
                    {
                        state[i] = DateTime.UtcNow;
                    }

                    if ("CreatedBy".Equals(propertyNames[i]) || "ModifiedBy".Equals(propertyNames[i]))
                    {
                        state[i] = Users.GetCurrentUser();
                    }
                }
                return true;
            }
            return false;
        }

    }
}
