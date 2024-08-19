

namespace EasyAnimator
{
     
    public interface IWrapper
    {
         

       
        object WrappedObject { get; }

         
    }


    public static partial class EasyAnimatorUtilities
    {
         

        public static object GetWrappedObject(object wrapper)
        {
            while (wrapper is IWrapper targetWrapper)
                wrapper = targetWrapper.WrappedObject;

            return wrapper;
        }

      
        public static bool TryGetWrappedObject<T>(object wrapper, out T wrapped) where T : class
        {
            while (true)
            {
                wrapped = wrapper as T;
                if (wrapped != null)
                    return true;

                if (wrapper is IWrapper targetWrapper)
                    wrapper = targetWrapper.WrappedObject;
                else
                    return false;
            }
        }

         
    }
}

