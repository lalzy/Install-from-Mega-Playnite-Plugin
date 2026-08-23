// ErrorHandler
using Playnite.SDK;
using System;

namespace InstallFromMegaPlugin{
    static public class ErrorHandler{
        ///<summary>Try/Catch wrapper that produce an error dialog on catch</summary>
        ///<param name = "tryBlock">The try block lambda</param>
        ///<param name = "api">PlayniteAPI instance</param>
        ///<param name = "error">Error Message to display in the dialog (before the exception object)</param>
        ///<param name = "catchBlock">The catch block lambda</param>
        ///<param name = "finallyBlock">The finallyBlock lambda</param>
        public static void WithTryCatch(Action tryBlock, IPlayniteAPI api, string error="Error", Action catchBlock=null, Action finallyBlock=null){
            try{
                tryBlock();
            }catch (Exception e){
                api.Dialogs.ShowErrorMessage($"{error}: {e}");
                if(catchBlock != null) catchBlock();
            }finally{
                if(finallyBlock != null) finallyBlock();
            }
        }

        ///<summary>Try/Catch wrapper with return support. Produce error dialog on catch</summary>
        ///<param name = "tryBlock">The try block lambda</param>
        ///<param name = "api">PlayniteAPI instance</param>
        ///<param name = "error">Error Message to display in the dialog (before the exception object)</param>
        ///<param name = "catchBlock">The catch block lambda</param>
        ///<param name = "finallyBlock">The finallyBlock lambda</param>
        ///<returns>The tryBlock return on success. Default on fail</returns>
        public static T WithTryCatchReturn<T>(Func<T> tryBlock, IPlayniteAPI api, string errorMessage="Error", Func<T>catchBlock=null, Func<T>finallyBlock=null){
            try{
                return tryBlock();
            }catch (Exception e){
                if(catchBlock != null) catchBlock();
                api.Dialogs.ShowErrorMessage($"{errorMessage}: {e}");
            }finally{
                if(finallyBlock != null) finallyBlock();
            }
            return default;
        }
    }
}
