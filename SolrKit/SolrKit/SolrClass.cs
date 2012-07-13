using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolrNet;
using SolrNet.Attributes;
using SolrNet.Commands;
using SolrNet.Impl;
using SolrNet.Utils;
using SolrNet.Mapping;
using SolrNet.Commands.Parameters;
using System.Drawing;

namespace SolrKit
{

    public class SolrDocument
    {

        [SolrField("text")]
        public string _text { get; set; }

        public double Score { get; set; }

    }


}
