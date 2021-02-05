using System;
using System.IO;
using System.Text;

namespace DHLib
{
    public abstract class CodeGenerator
    {
        // AccessModifier
        protected enum AccessMode
        {
            Private,
            Protected,
            Internal,
            Public,
        }

        protected struct Scope : IDisposable
        {
            Action mOnClosed;

            public Scope(Action onStart, Action onClosed)
            {
                if ( onStart != null )
                    onStart.Invoke();

                mOnClosed = onClosed;
            }

            public void Dispose()
            {
                if ( mOnClosed != null )
                {
                    mOnClosed.Invoke();
                    mOnClosed = null;
                }
            }
        }

        protected class CodeEmitter
        {
            StringBuilder mBuilder;
            int mIndentLevel;

            public CodeEmitter(int capacity)
            {
                mBuilder = new StringBuilder( capacity );
                mIndentLevel = 0;
            }

            public CodeEmitter Line()
            {
                mBuilder.AppendLine();
                return this;
            }

            public CodeEmitter Append(char value)
            {
                mBuilder.Append( value );
                return this;
            }

            public CodeEmitter Append(string value)
            {
                mBuilder.Append( value );
                return this;
            }

            public CodeEmitter Tab()
            {
                if ( mIndentLevel > 0 )
                    mBuilder.Append( '\t', mIndentLevel );
                return this;
            }

            public CodeEmitter Tab(char value)
            {
                if ( mIndentLevel > 0 )
                    mBuilder.Append( '\t', mIndentLevel );
                mBuilder.Append( value );
                return this;
            }

            public CodeEmitter Tab(string value)
            {
                if ( mIndentLevel > 0 )
                    mBuilder.Append( '\t', mIndentLevel );
                mBuilder.Append( value );
                return this;
            }

            public CodeEmitter Join(params string[] args)
            {
                if ( args.Length == 0 )
                    return this;

                mBuilder.Append( args[0] );

                for (int i = 1; i < args.Length; ++i )
                {
                    mBuilder.Append( ' ' );
                    mBuilder.Append( args[i] );
                }

                return this;
            }

            public CodeEmitter Join(AccessMode access, params string[] args)
            {
                string strAccess = null;
                switch ( access )
                {
                    case AccessMode.Public:
                        strAccess = "public";
                        break;
                    case AccessMode.Private:
                        //strAccess = "private";
                        break;
                    case AccessMode.Protected:
                        strAccess = "protected";
                        break;
                    case AccessMode.Internal:
                        strAccess = "internal";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if ( strAccess != null )
                {
                    // NOTE: 이 부분 하드코딩 해놓았으므로 Join 함수내 seperator변경시 같이 수정할 것.
                    mBuilder.Append( strAccess );
                    mBuilder.Append( ' ' );
                }
                Join( args );
                return this;
            }

            public Scope Brace()
            {
                return new Scope(
                    () => { Tab( '{' ).Line(); mIndentLevel++; },
                    () => { mIndentLevel--; Tab( '}' ).Line(); }
                    );
            }

            public override string ToString()
            {
                return mBuilder.ToString();
            }
        }

        CodeEmitter mEmitter;

        public CodeGenerator()
        {
            // 512kb.
            mEmitter = new CodeEmitter( capacity: 1024 * 512 ); 
        }

        protected void Header(string className)
        {
            Using( "System" );
            Using( "System.Collections" );
            Using( "System.Collections.Generic" );
            Using( "DHLib" );
            NewLine( 1 );
            Comment( "-------------------------------------------------" );
            Comment( $"This code is automatically generated from {className}, which inherited {nameof( CodeGenerator )}" );
            Comment( "DO NOT EDIT THE CODE MANUALLY." );
            Comment( $"Time: {DateTime.Now}" );
            Comment( "Author: donghyeon yoo" );
            Comment( "-------------------------------------------------" );
            NewLine( 1 );
        }

        protected void Comment(string value)
        {
            // \t // <value>
            mEmitter.Tab( "// " ).Append( value ).Line();
        }

        protected void Using(string value)
        {
            // using <value>;
            mEmitter.Join( "using", value ).Append( ';' ).Line();
        }

        protected Scope Namespace(string name)
        {
            //\t namespace <name> \n
            mEmitter.Tab().Join( "namespace", name ).Line();
            return mEmitter.Brace();
        }

        protected Scope Enum(AccessMode access, string name, string underlyingType = null)
        {
            //\t <access> enum <name> : <underlyingType> \n{
            mEmitter.Tab().Join( access, "enum", name );
            if ( underlyingType != null )
                mEmitter.Join( " :", underlyingType );
            mEmitter.Line();
            return mEmitter.Brace();
        }

        protected void EnumConstant(string name, string specific = null)
        {
            //\t <name> = <specific>, \n
            mEmitter.Tab().Append( name );
            if ( specific != null )
                mEmitter.Join( " = ", specific );
            mEmitter.Append( ',' ).Line();
        }

        protected Scope Interface(AccessMode access, string name)
        {
            //\t <access> interface <name> \n
            mEmitter.Tab().Join( access, "interface", name ).Line();
            return mEmitter.Brace();
        }

        protected Scope Class(AccessMode access, string name)
        {
            //\t <access> class <name> \n
            mEmitter.Tab().Join( access, "class", name ).Line();
            return mEmitter.Brace();
        }

        protected Scope Class(AccessMode access, string name, string @interface)
        {
            //\t <access> class <name> : @interface \n
            mEmitter.Tab().Join( access, "class", name, ":", @interface ).Line();
            return mEmitter.Brace();
        }

        protected void Member(AccessMode access, string type, string name)
        {
            //\t <access> <type> <name>; \n
            mEmitter.Tab().Join( access, type, name ).Append( ';' ).Line();
        }

        protected void MemberList(AccessMode access, string valueType, string name)
        {
            //\t <access> List<valueType> <name>; \n
            mEmitter.Tab().Join( access ).Append( "List<" ).Append( valueType ).Append( "> " ).Append( name ).Append( ';' ).Line();
        }

        protected void MemberDictionary(AccessMode access, string keyType, string valueType, string name)
        {
            //\t <access> Dictionary<keyType, valueType> <name>; \n
            mEmitter.Tab().Join( access ).Append( "Dictionary<" ).Append( keyType ).Join( ",", valueType ).Append( "> " ).Append( name ).Append( ';' ).Line();
        }

        protected void GetOnlyField(AccessMode access, string type, string name)
        {
            //\t <access> <type> <name> { get; } \n
            mEmitter.Tab().Join( access, type, name ).Append( " { get; }" ).Line();
        }

        protected Scope Construct(AccessMode access, string name, params string[] args)
        {
            //\t <access> <name>(<args>){ \n
            mEmitter.Tab().Join( access, name ).Append( "(" ).Join( args ).Append( ")" ).Line();
            return mEmitter.Brace();
        }

        protected Scope Method(AccessMode access, string name, string @return = null, params string[] args)
        {
            //\t <access> <@return> <name>(<args>){ \n
            if ( @return == null )
                @return = "void";
            mEmitter.Tab().Join( access, @return, name ).Append( "(" ).Join( args ).Append( ")" ).Line();
            return mEmitter.Brace();
        }

        protected void Write(string value)
        {
            mEmitter.Tab( value );
        }

        protected void WriteLine(string value)
        {
            mEmitter.Tab( value ).Line();
        }

        protected Scope Brace(string value)
        {
            mEmitter.Tab( value ).Line();
            return mEmitter.Brace();
        }

        protected void NewLine(int count)
        {
            for ( ; count > 0; --count )
                mEmitter.Line();
        }

        public void CreateFile(string path)
        {
            if ( !path.EndsWith(".cs") )
                path = path + ".cs";

            using ( var file = File.Create( path ) )
            using ( var stream = new StreamWriter(file) )
            {
                stream.Write( mEmitter.ToString() );
            }
        }
    }
}