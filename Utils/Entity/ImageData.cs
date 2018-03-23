using System;

namespace Insight.Utils.Entity
{
    public class ImageData
    {
        /// <summary>
        /// ΨһID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// ����ID
        /// </summary>
        public string categoryId { get; set; }

        /// <summary>
        /// Ӱ������
        /// </summary>
        public int imageType { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// ��չ��
        /// </summary>
        public string expand { get; set; }

        /// <summary>
        /// ���ܵȼ�
        /// </summary>
        public string secrecyDegree { get; set; }

        /// <summary>
        /// ҳ��
        /// </summary>
        public int? pages { get; set; }

        /// <summary>
        /// �ļ��ֽ���
        /// </summary>
        public long? size { get; set; }

        /// <summary>
        /// �ļ�·��
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Ӱ������
        /// </summary>
        public byte[] image { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// �Ƿ�ʧЧ
        /// </summary>
        public bool isInvalid { get; set; }

        /// <summary>
        /// ��������ID
        /// </summary>
        public string creatorDeptId { get; set; }

        /// <summary>
        /// ������ID
        /// </summary>
        public string creatorId { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime createTime { get; set; }
    }
}
