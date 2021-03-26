<%@ Page Language="C#" AutoEventWireup="true" CodeFile="uploadImage.aspx.cs" Inherits="uploadImage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:Image ID="Image1" runat="server" Height="300px" Width="200px" />
        <br />
        <asp:FileUpload ID="FileUpload1" runat="server" />
        <br />
        Input Name :         <asp:TextBox ID="txtInput" runat="server"></asp:TextBox>
        <br />
        IImage Url :
        <asp:Label ID="lblUrl" runat="server"></asp:Label>
        <br />
        <asp:Button ID="btnSubmit" runat="server" onclick="btnSubmit_Click" 
            Text="Submit" />
    
    </div>
    </form>
</body>
</html>
