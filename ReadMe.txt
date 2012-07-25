To get started with the Web Mapping Application: 

1. Open Default.aspx in Design mode.

2. Right-click the MapResourceManager control and click Edit Resources. 

3. Click the Add button to add a MapResourceItem to the collection. 
  
4. Click the Definition property and click the ellipsis (...) button. 

5. From the Type dropdown list, choose the type of resource that you want to add. 

6. Use the ellipses (...) buttons next to the Data Source, Identity, and Resource boxes to display dialogs that will help you specify the information necessary to display the resource. 

Note: If you are adding an ArcGIS Local resource, you set the identity by right-clicking the Web site name in the Solution Explorer and clicking Add ArcGIS Identity. The account you specify must have Write access to your Temporary ASP.NET Files directory.

7. If your map will contain more than one resource, repeat steps 3-6. You may need to adjust transparency levels using the MapResourceItems' DisplaySettings properties. 

8. When you are finished adding resources and configuring their display settings, click OK to exit the MapResourceItem Collection Editor.

9. Click the OverviewMap control. In the Properties grid, set the OverviewMapResource property to the resource you want to appear in the overview map.

10. Run the application.

For further details about each part of the Web Mapping Application, see the section “Developing Web applications using the Web ADF” in the ArcGIS Server Developer Help. 