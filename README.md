# SobekCM-plugin-endpoint-AGGCOUNTS
<p>SobekCM-plugin-endpoint-AGGCOUNTS is a plugin for the open-source SobekCM Digital Repository software (Mark V. Sullivan, 
lead developer). It provides a REST API endpoint to return a list of the aggregations with various item counts.</p>
<p>It is intended that be consumed by a future System Admin/Collections report (via another plugin).</p>
<p>engine URL: /engine/plugins/aggcounts/xml</p>
<h3>Current values returned</h3>
<ul>
<li>code: aggregation code</li>
<li>name: aggregatio name</li>
<li>id: aggregation id</li>
<li>type: aggregation type</li>
<li>isActive</li>
<li>isHidden</li>
<li>count_public</li>
<li>count_private</li>
<li>count_dark</li>
<li>indexed_count</li>
<li>alias</li>
</ul>
<hr/>
<p>Richard Bernardy - <a href="mailto:rbernard@usf.edu">rbernard@usf.edu</a> - January 9, 2017</p>
