namespace NetProtocolCodeGen.Editor.Generator.Utils.Directives
{
    public class UsingDirectivesHolder
    {
        private static UsingDirectivesHolder _instance;

        public static UsingDirectivesHolder Instance => _instance ?? (_instance = new UsingDirectivesHolder());

        private UsingDirectivesHolder()
        {
            
        }
        
        public IUsingDirectives UsingDirectives { private set; get; }

        public void SetUsingDirectives(IUsingDirectives usingDirectives)
        {
            UsingDirectives = usingDirectives;
        }
    }
}