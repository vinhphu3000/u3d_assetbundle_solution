
using System;
using Sproto;
using System.Collections.Generic;

namespace proto
{

    public enum Test12 {  
        OK = 1, //成功   
        ERROR = 2,  //error   
        OTHER = 3,  //其他  
    }



    public class TestProtoClass1 : SprotoTypeBase {
        private static int max_field_count = 6;

        [SprotoHasField]
        public bool HasA{
            get { return base.has_field.has_field(0); }
        }

        private Int32 _A; // tag 0 
        public Int32 A { //注释测试1 
            get{ return _A; }
            set{ base.has_field.set_field(0,true); _A = value; }
        }

        [SprotoHasField]
        public bool HasB{
            get { return base.has_field.has_field(1); }
        }

        private bool _B; // tag 1 
        public bool B { 
            get{ return _B; }
            set{ base.has_field.set_field(1,true); _B = value; }
        }

        [SprotoHasField]
        public bool HasC{
            get { return base.has_field.has_field(2); }
        }

        private Int64 _C; // tag 2 
        public Int64 C { 
            get{ return _C; }
            set{ base.has_field.set_field(2,true); _C = value; }
        }

        [SprotoHasField]
        public bool HasD{
            get { return base.has_field.has_field(3); }
        }

        private Int32 _D; // tag 3 
        public Int32 D { 
            get{ return _D; }
            set{ base.has_field.set_field(3,true); _D = value; }
        }

        [SprotoHasField]
        public bool HasE{
            get { return base.has_field.has_field(4); }
        }

        private List<Int64> _E; // tag 4 
        public List<Int64> E { 
            get{ return _E; }
            set{ base.has_field.set_field(4,true); _E = value; }
        }

        [SprotoHasField]
        public bool HasF{
            get { return base.has_field.has_field(5); }
        }

        private string _F; // tag 5 
        public string F { 
            get{ return _F; }
            set{ base.has_field.set_field(5,true); _F = value; }
        }


        public TestProtoClass1() : base(max_field_count) {}

        public TestProtoClass1(byte[] buffer) : base(max_field_count, buffer) {
            this.decode ();
        } 

        protected override void decode () {
            int tag = -1;
            while (-1 != (tag = base.deserialize.read_tag ())) {
                switch (tag) {  

                    case 0:
                        this.A = base.deserialize.read_int32();
                        break;

                    case 1:
                        this.B = base.deserialize.read_boolean();
                        break;

                    case 2:
                        this.C = base.deserialize.read_int64();
                        break;

                    case 3:
                        this.D = base.deserialize.read_int32();
                        break;

                    case 4:
                        this.E = base.deserialize.read_int64_list();
                        break;

                    case 5:
                        this.F = base.deserialize.read_string();
                        break;

                    default:
                        base.deserialize.read_unknow_data ();
                        break;
                }
            }
        }

        public override int encode (SprotoStream stream) {
            base.serialize.open (stream);

            if (base.has_field.has_field (0)) {
                base.serialize.write_int32(this.A, 0);
            } 

            if (base.has_field.has_field (1)) {
                base.serialize.write_boolean(this.B, 1);
            } 

            if (base.has_field.has_field (2)) {
                base.serialize.write_int64(this.C, 2);
            } 

            if (base.has_field.has_field (3)) {
                base.serialize.write_int32(this.D, 3);
            } 

            if (base.has_field.has_field (4)) {
                base.serialize.write_int64(this.E, 4);
            } 

            if (base.has_field.has_field (5)) {
                base.serialize.write_string(this.F, 5);
            } 

            return base.serialize.close ();
        }
    } 

    public class TestProtoClass2 : SprotoTypeBase {
        private static int max_field_count = 3;

        [SprotoHasField]
        public bool HasA{
            get { return base.has_field.has_field(0); }
        }

        private byte[] _A; // tag 0 
        public byte[] A { //二进制测试 
            get{ return _A; }
            set{ base.has_field.set_field(0,true); _A = value; }
        }

        [SprotoHasField]
        public bool HasB{
            get { return base.has_field.has_field(1); }
        }

        private TestProtoClass1 _B; // tag 1 
        public TestProtoClass1 B { 
            get{ return _B; }
            set{ base.has_field.set_field(1,true); _B = value; }
        }

        [SprotoHasField]
        public bool HasC{
            get { return base.has_field.has_field(2); }
        }

        private List<TestProtoClass1> _C; // tag 2 
        public List<TestProtoClass1> C { 
            get{ return _C; }
            set{ base.has_field.set_field(2,true); _C = value; }
        }


        public TestProtoClass2() : base(max_field_count) {}

        public TestProtoClass2(byte[] buffer) : base(max_field_count, buffer) {
            this.decode ();
        } 

        protected override void decode () {
            int tag = -1;
            while (-1 != (tag = base.deserialize.read_tag ())) {
                switch (tag) {  

                    case 0:
                        this.A = base.deserialize.read_bytes();
                        break;

                    case 1:
                        this.B = base.deserialize.read_obj<TestProtoClass1>();
                        break;

                    case 2:
                        this.C = base.deserialize.read_obj_list<TestProtoClass1>();
                        break;

                    default:
                        base.deserialize.read_unknow_data ();
                        break;
                }
            }
        }

        public override int encode (SprotoStream stream) {
            base.serialize.open (stream);

            if (base.has_field.has_field (0)) {
                base.serialize.write_bytes(this.A, 0);
            } 

            if (base.has_field.has_field (1)) {
                base.serialize.write_obj(this.B, 1);
            } 

            if (base.has_field.has_field (2)) {
                base.serialize.write_obj<TestProtoClass1>(this.C, 2);
            } 

            return base.serialize.close ();
        }
    } 

}
