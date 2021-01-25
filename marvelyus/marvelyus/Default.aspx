<%@ Page Title="Marvelyus" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="marvelyus._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h3>Table seçip çalıştıra basınız.</h3>
         <div> <asp:Label ID="Label1" runat="server" CssClass="btn-danger" Text=""></asp:Label></div>
        <asp:CheckBox ID="CheckBox1" runat="server" Enabled="true" Text="Hepsini seç" AutoPostBack="true" OnCheckedChanged="CheckBox1_CheckedChanged" />
        <asp:CheckBoxList ID="CheckBoxList1" runat="server" Font-Size="Small" Font-Bold="false" RepeatDirection="Vertical"></asp:CheckBoxList>
        <p>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" class="btn btn-primary btn-lg" Text="Çalıştır" />

        </p>
    </div>

</asp:Content>
