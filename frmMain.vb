﻿' Main form for all processing
Imports System.Data.SQLite
Imports System.Globalization
Imports System.Threading
Imports System.IO
Imports System.Net
Imports MoreLinq.MoreEnumerable

Public Class frmMain
    Inherits System.Windows.Forms.Form

    ' Update Prices Variables
    Private m_ControlsCollection As ControlsCollection
    Private RegionCheckBoxes() As CheckBox
    Private SystemCheckBoxes() As CheckBox
    Private TechCheckBoxes() As CheckBox
    ' For saving the price type that was used in the download
    Private GroupPricesList As New List(Of GroupPriceType)
    Private GroupPriceTypetoFind As New GroupPriceType
    Private Class GroupPriceType
        Public PriceType As String
        Public ItemID As Long

        Public Sub New()
            PriceType = ""
            ItemID = 0
        End Sub
    End Class

    ' Datacores
    Private DCSkillCheckBoxes() As CheckBox
    Private DCSkillLabels() As Label
    Private DCSkillCombos() As ComboBox
    Private DCCorpCheckBoxes() As CheckBox
    Private DCCorpLabels() As Label
    Private DCCorpTextboxes() As TextBox

    ' Mining ore processing skills
    Private MineProcessingCheckBoxes() As CheckBox
    Private MineProcessingLabels() As Label
    Private MineProcessingCombos() As ComboBox

    ' Manufacturing
    Private CalcRelicCheckboxes() As CheckBox
    Private CalcDecryptorCheckBoxes() As CheckBox
    Private TypeIDToFind As Long ' For searching a price list

    ' Manufacturing Column stuff
    Private ColumnPositions(NumManufacturingTabColumns) As String ' For saving the column order
    Private AddingColumns As Boolean
    Private MovedColumn As Integer

    Private DCIPH_COLUMN As Integer ' The number of the DC IPH column for totalling up the price

    Private TechChecked As Boolean
    Private RunUpdatePriceList As Boolean = True ' If we want to run the price list update
    Private RefreshList As Boolean = True
    Private UpdateAllTechChecks As Boolean = True ' Whether to update all Tech checks in prices or not
    Private FirstShowMining As Boolean = True ' If we have clicked on the Mining tab yet to load initial data
    Private FirstShowDatacores As Boolean = True ' If we have clicked on the Datacores tab yet or not (only load initial data on first click)

    ' Blueprints Variables
    Private cmbBPsLoaded As Boolean

    ' BP List for Previous/Next
    Private BPHistory As New List(Of BPHistoryItem)

    Private Structure BPHistoryItem
        Dim BPID As Long
        Dim BPName As String
        Dim BuildType As String
        Dim Inputs As String
        Dim SentFrom As SentFromLocation
        Dim BuildFacility As IndustryFacility
        Dim ComponentFacility As IndustryFacility
        Dim CapComponentFacility As IndustryFacility
        Dim InventionFacility As IndustryFacility
        Dim CopyFacility As IndustryFacility
        Dim IncludeTaxes As Boolean
        Dim IncludeFees As Boolean
        Dim MEValue As String
        Dim TEValue As String
        Dim SentRuns As String
        Dim ManufacturingLines As String
        Dim LabLines As String
        Dim NumBPs As String
        Dim AddlCosts As String
        Dim PPU As Boolean
    End Structure

    Private CurrentBPHistoryIndex As Integer
    Private ResetBPTab As Boolean ' If we recalled the InitBP to enable all the stuff on the screen

    ' BP Combo processing
    Public ComboMenuDown As Boolean
    Public MouseWheelSelection As Boolean
    Public ComboBoxArrowKeys As Boolean
    Public BPSelected As Boolean
    Public BPComboKeyDown As Boolean

    ' Relics
    Private LoadingRelics As Boolean
    Private RelicsLoaded As Boolean

    ' Decryptors
    Private SelectedDecryptor As New Decryptor

    ' Invention
    Private UpdatingInventionChecks As Boolean

    Private LoadingInventionDecryptors As Boolean
    Private LoadingT3Decryptors As Boolean

    Private InventionDecryptorsLoaded As Boolean
    Private T3DecryptorsLoaded As Boolean

    ' If we are loading from history
    Private LoadingBPfromHistory As Boolean

    ' Updates for threading
    Public PriceHistoryUpdateCount As Integer
    Public PriceOrdersUpdateCount As Integer

    ' BP stats
    Private OwnedBP As Boolean

    ' For T2 BPOs mainly so we can load the stored ME/TE if it changes
    Private OwnedBPME As String
    Private OwnedBPPE As String

    Private UpdatingCheck As Boolean

    ' For checks that are enabled
    Private PriceCheckT1Enabled As Boolean
    Private PriceCheckT2Enabled As Boolean
    Private PriceCheckT3Enabled As Boolean
    Private PriceCheckT4Enabled As Boolean
    Private PriceCheckT5Enabled As Boolean
    Private PriceCheckT6Enabled As Boolean

    ' For updating several checks at once
    Private IgnoreSystemCheckUpdates As Boolean
    Private IgnoreRegionCheckUpdates As Boolean

    Private PriceToggleButtonHit As Boolean

    ' Total isk per hour selected on datacore grid
    Private TotalSelectedIPH As Double

    ' Loading the solar system combo on the price page
    Private FirstSolarSystemComboLoad As Boolean
    Private FirstPriceShipTypesComboLoad As Boolean
    Private FirstPriceChargeTypesComboLoad As Boolean

    ' For loading the Manufacturing Grid
    Private FirstLoadCalcBPTypes As Boolean
    Private FirstManufacturingGridLoad As Boolean

    Private UserInventedBPs As New List(Of Long)  ' This is a list of all the BPs that the user will invent from owned T1s (Manufacturing Grid)

    ' For ignoring updates to the ship booster combo in mining
    Private UpdatingMiningShips As Boolean

    ' The Reaction list for Reactions tab
    Private GlobalReactionList As New List(Of Reaction)

    ' If we refresh the manufacturing data or recalcuate
    Private RefreshCalcData As Boolean

    ' Reload of Regions on Datacore class
    Private DCRegionsLoaded As Boolean

    ' Final list of items for manufacturing (keep form level so we can refresh it if needed)
    Private FinalManufacturingItemList As List(Of ManufacturingItem)
    Private ManufacturingRecordIDToFind As Long ' for Predicate
    Private ManufacturingNameToFind As String ' for Predicate

    ' For column sorting - What column did they click on to sort
    Private ManufacturingColumnClicked As Integer
    Private ManufacturingColumnSortType As SortOrder
    Private UpdatePricesColumnClicked As Integer
    Private UpdatePricesColumnSortType As SortOrder
    Private BPCompColumnClicked As Integer
    Private BPCompColumnSortType As SortOrder
    Private BPRawColumnClicked As Integer
    Private BPRawColumnSortType As SortOrder
    Private DCColumnClicked As Integer
    Private DCColumnSortType As SortOrder
    Private MiningColumnClicked As Integer
    Private MiningColumnSortType As SortOrder
    Private ReactionsColumnClicked As Integer
    Private ReactionsColumnSortType As SortOrder

    ' For maximum production and laboratory lines
    Private MaximumProductionLines As Integer
    Private MaximumLaboratoryLines As Integer

    Private SelectedBPText As String = ""

    Private ProfitText As String
    Private ProfitPercentText As String

    Private DefaultSettings As New ProgramSettings ' For default constants

    Private ListIDIterator As Integer ' For setting a unique record id in the manufacturing tab
    Private ListRowFormats As New List(Of RowFormat) ' The lists of formats to use in drawing the manufacturing list

    Private SelectedBPTabIndex As Integer ' So we don't move around the different facility/invention/re tabs on the user

    ' Inline grid row update variables
    Private Structure SavedLoc
        Dim X As Integer
        Dim Y As Integer
    End Structure

    Private SavedListClickLoc As SavedLoc
    Private RefreshingGrid As Boolean

    Private CurrentRow As ListViewItem
    Private PreviousRow As ListViewItem
    Private NextRow As ListViewItem

    Private NextCellRow As ListViewItem
    Private PreviousCellRow As ListViewItem

    Private CurrentCell As ListViewItem.ListViewSubItem
    Private PreviousCell As ListViewItem.ListViewSubItem
    Private NextCell As ListViewItem.ListViewSubItem

    Private MEUpdate As Boolean
    Private PriceUpdate As Boolean
    Private DataUpdated As Boolean
    Private DataEntered As Boolean
    Private EnterKeyPressed As Boolean
    Private SelectedGrid As ListView

    Private PriceTypeUpdate As Boolean
    Private PriceSystemUpdate As Boolean
    Private PriceRegionUpdate As Boolean
    Private PriceModifierUpdate As Boolean
    Private PreviousPriceType As String
    Private PreviousRegion As String
    Private PreviousSystem As String
    Private PreviousPriceMod As String
    Private TabPressed As Boolean
    Private UpdatingCombo As Boolean

    Private PPRawSystemsLoaded As Boolean
    Private DefaultPreviousRawRegion As String
    Private PPItemsSystemsLoaded As Boolean
    Private DefaultPreviousItemsRegion As String

    Private IgnoreFocus As Boolean
    Private IgnoreMarketFocus As Boolean

    ' Column width consts - may change depending on Ore, Ice or Gas so change the widths of the columns based on these and use them to add and move
    Private Const MineOreNameColumnWidth As Integer = 120
    Private Const MineRefineYieldColumnWidth As Integer = 70
    Private Const MineCrystalColumnWidth As Integer = 45
    Private Const PriceListHeaderCSV As String = "Group Name,Item Name,Price,Price Type,Raw Material,Type ID"
    Private Const PriceListHeaderTXT As String = "Group Name|Item Name|Price|Price Type|Raw Material|Type ID"
    Private Const PriceListHeaderSSV As String = "Group Name;Item Name;Price;Price Type;Raw Material;Type ID"

#Region "Initialization Code"

    ' Set default window theme so tabs in invention window display correctly on all systems
    Public Declare Unicode Function SetWindowTheme Lib "uxtheme.dll" (ByVal hWnd As IntPtr, _
        ByVal pszSubAppName As String, ByVal pszSubIdList As String) As Integer

    Public Sub New()

        MyBase.New()

        Dim ErrorData As ErrObject = Nothing
        Dim ESIData As New ESI

        ErrorTracker = ""

        ' Set developer flag
        If File.Exists("Developer.txt") Then
            Developer = True
        Else
            Developer = False
        End If

        ' Set test platform
        If File.Exists("Test.txt") Then
            TestingVersion = True
        Else
            TestingVersion = False
        End If

        Call SetProgress("Initializing...")

        Application.DoEvents()

        ' This call is required by the designer.
        InitializeComponent()

        FirstLoad = True

        ' Always use US for now and don't take into account user overrided stuff like the system clock format
        LocalCulture = New CultureInfo("en-US", False)
        ' Sets the CurrentCulture 
        Thread.CurrentThread.CurrentCulture = LocalCulture

        ' Add any initialization after the InitializeComponent() call.

        ' Find out if we are running all in the current folder or with updates and the DB in the appdata folder
        If File.Exists(SQLiteDBFileName) Then
            ' Single folder that we are in, so set the path variables to it for updates
            DynamicFilePath = Path.GetDirectoryName(Application.ExecutablePath)
            DBFilePath = Path.GetDirectoryName(Application.ExecutablePath)
        Else
            ' They ran the installer (or we assume they did) and all the files are updated in the appdata/roaming folder
            ' Set where files will be updated
            DynamicFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DynamicAppDataPath)
            DBFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DynamicAppDataPath)
        End If

        ' Get the user settings then check for updates
        UserApplicationSettings = AllSettings.LoadApplicationSettings

        ' Check for program updates first
        If UserApplicationSettings.CheckforUpdatesonStart Then
            ' Check for program updates
            Application.UseWaitCursor = True
            Me.Activate()
            Call CheckForUpdates(False, Me.Icon)
            Application.UseWaitCursor = False
            Application.DoEvents()
        End If

        ' Initialize stuff
        Call SetProgress("Initializing Database...")
        Application.DoEvents()
        EVEDB = New DBConnection(Path.Combine(DBFilePath, SQLiteDBFileName))

        ' For speed on ESI calls
        ServicePointManager.DefaultConnectionLimit = 20
        ServicePointManager.UseNagleAlgorithm = False
        ServicePointManager.Expect100Continue = False

        ' Load the user settings
        Call SetProgress("Loading User Settings...")
        UserBPTabSettings = AllSettings.LoadBPSettings
        UserUpdatePricesTabSettings = AllSettings.LoadUpdatePricesSettings
        UserManufacturingTabSettings = AllSettings.LoadManufacturingSettings
        UserDCTabSettings = AllSettings.LoadDatacoreSettings
        UserReactionTabSettings = AllSettings.LoadReactionSettings
        UserMiningTabSettings = AllSettings.LoadMiningSettings
        UserIndustryJobsColumnSettings = AllSettings.LoadIndustryJobsColumnSettings
        UserManufacturingTabColumnSettings = AllSettings.LoadManufacturingTabColumnSettings
        UserShoppingListSettings = AllSettings.LoadShoppingListSettings
        UserMHViewerSettings = AllSettings.LoadMarketHistoryViewerSettingsSettings
        UserBPViewerSettings = AllSettings.LoadBPViewerSettings
        UserUpwellStructureSettings = AllSettings.LoadUpwellStructureViewerSettings
        StructureBonusPopoutViewerSettings = AllSettings.LoadStructureBonusPopoutViewerSettings

        UserIndustryFlipBeltSettings = AllSettings.LoadIndustryFlipBeltColumnSettings
        UserIndustryFlipBeltOreCheckSettings1 = AllSettings.LoadIndustryBeltOreChecksSettings(BeltType.Small)
        UserIndustryFlipBeltOreCheckSettings2 = AllSettings.LoadIndustryBeltOreChecksSettings(BeltType.Medium)
        UserIndustryFlipBeltOreCheckSettings3 = AllSettings.LoadIndustryBeltOreChecksSettings(BeltType.Large)
        UserIndustryFlipBeltOreCheckSettings4 = AllSettings.LoadIndustryBeltOreChecksSettings(BeltType.Enormous)
        UserIndustryFlipBeltOreCheckSettings5 = AllSettings.LoadIndustryBeltOreChecksSettings(BeltType.Colossal)

        UserAssetWindowManufacturingTabSettings = AllSettings.LoadAssetWindowSettings(AssetWindow.ManufacturingTab)
        UserAssetWindowShoppingListSettings = AllSettings.LoadAssetWindowSettings(AssetWindow.ShoppingList)
        UserAssetWindowDefaultSettings = AllSettings.LoadAssetWindowSettings(AssetWindow.DefaultView)

        ' Load the character
        Call SetProgress("Loading Character Data from ESI...")
        Call LoadCharacter(UserApplicationSettings.LoadAssetsonStartup, UserApplicationSettings.LoadBPsonStartup)

        ' Only allow selecting a character if they registered the program
        If AppRegistered() Then
            mnuSelectionAddChar.Enabled = True
        Else
            mnuSelectionAddChar.Enabled = False
        End If

        Call LoadCharacterNamesinMenu()

        ' Type of skills loaded
        Call UpdateSkillPanel()

        If Not IsNothing(SelectedCharacter.Skills) Then ' 3387 mass production, 24625 adv mass production, 3406 laboratory efficiency, 24524 adv laboratory operation
            MaximumProductionLines = SelectedCharacter.Skills.GetSkillLevel(3387) + SelectedCharacter.Skills.GetSkillLevel(24625) + 1
            MaximumLaboratoryLines = SelectedCharacter.Skills.GetSkillLevel(3406) + SelectedCharacter.Skills.GetSkillLevel(24624) + 1
        Else
            MaximumProductionLines = 1
            MaximumLaboratoryLines = 1
        End If

        ' ESI Facilities
        If UserApplicationSettings.LoadESIFacilityDataonStartup Then
            ' Always do cost indicies first
            Application.UseWaitCursor = True
            Call SetProgress("Updating Industry Facilities...")
            Application.DoEvents()
            Call ESIData.UpdateIndustryFacilties(Nothing, Nothing, True)
            Application.UseWaitCursor = False
            Application.DoEvents()
        End If

        DBCommand = Nothing

        ' ESI Market Data
        If UserApplicationSettings.LoadESIMarketDataonStartup Then
            Application.UseWaitCursor = True
            Application.DoEvents()
            Call SetProgress("Updating Avg/Adj Market Prices...")
            Call ESIData.UpdateAdjAvgMarketPrices()
            Application.UseWaitCursor = False
            Application.DoEvents()
        End If

        If TestingVersion Then
            Me.Text = Me.Text & " - Testing"
        End If

        If Developer Then
            Me.Text = Me.Text & " - Developer"
            mnuInventionSuccessMonitor.Visible = True
            mnuFactoryFinder.Visible = True
            mnuMarketFinder.Visible = True
            mnuRefinery.Visible = True
            mnuLPStore.Visible = True
        Else
            ' Hide all the development stuff
            mnuInventionSuccessMonitor.Visible = False
            mnuFactoryFinder.Visible = False
            mnuMarketFinder.Visible = False
            mnuRefinery.Visible = False
            mnuLPStore.Visible = False
            tabMain.TabPages.Remove(tabPI)
            tabMain.TabPages.Remove(tabReactions)
        End If

        ' Initialize the BP facility
        Call BPTabFacility.InitializeControl(FacilityView.FullControls, SelectedCharacter.ID, ProgramLocation.BlueprintTab, ProductionType.Manufacturing)

        ' Load up the Manufacturing tab facilities
        Call CalcBaseFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.Manufacturing)
        Call CalcInventionFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.Invention)
        Call CalcT3InventionFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.T3Invention)
        Call CalcCopyFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.Copying)
        Call CalcSupersFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.SuperManufacturing)
        Call CalcCapitalsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.CapitalManufacturing)
        Call CalcSubsystemsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.SubsystemManufacturing)
        Call CalcReactionsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.Reactions)
        Call CalcBoostersFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.BoosterManufacturing)

        ' Two facilities with check options - load the one they save
        If UserManufacturingTabSettings.CheckCapitalComponentsFacility Then
            Call CalcComponentsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.CapitalComponentManufacturing)
        Else
            Call CalcComponentsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.ComponentManufacturing)
        End If

        If UserManufacturingTabSettings.CheckT3DestroyerFacility Then
            Call CalcT3ShipsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.T3DestroyerManufacturing)
        Else
            Call CalcT3ShipsFacility.InitializeControl(FacilityView.LimitedControls, SelectedCharacter.ID, ProgramLocation.ManufacturingTab, ProductionType.T3CruiserManufacturing)
        End If

        ' Init Tool tips
        If UserApplicationSettings.ShowToolTips Then
            Me.ttBP = New ToolTip(Me.components)
            Me.ttBP.IsBalloon = True
        End If

        ' Nothing in shopping List
        pnlShoppingList.Text = "No Items in Shopping List"

        Call SetProgress("Finalizing Forms...")

        '****************************************
        '**** Blueprints Tab Initializations ****
        '****************************************
        ' Width is now 556, scrollbar is 21 
        'lstBPComponentMats.Columns.Add("", -2, HorizontalAlignment.Center) ' For check (25 size)
        lstBPComponentMats.Columns.Add("Material", 225, HorizontalAlignment.Left) 'added 25 temp
        lstBPComponentMats.Columns.Add("Quantity", 80, HorizontalAlignment.Right)
        lstBPComponentMats.Columns.Add("ME", 35, HorizontalAlignment.Center)
        lstBPComponentMats.Columns.Add("Cost Per Item", 90, HorizontalAlignment.Right)
        lstBPComponentMats.Columns.Add("Total Cost", 105, HorizontalAlignment.Right)

        ' No check for raw mats since the check will be used to toggle build/buy for each item
        lstBPRawMats.Columns.Add("Material", 210, HorizontalAlignment.Left)
        lstBPRawMats.Columns.Add("Quantity", 90, HorizontalAlignment.Right)
        lstBPRawMats.Columns.Add("ME", 35, HorizontalAlignment.Center)
        lstBPRawMats.Columns.Add("Cost Per Item", 90, HorizontalAlignment.Right)
        lstBPRawMats.Columns.Add("Total Cost", 110, HorizontalAlignment.Right)

        ' We haven't checked any tech levels yet
        TechChecked = False

        Call InitBPTab()

        Call InitInventionTab()

        ' Base Decryptor
        SelectedDecryptor.MEMod = 0
        SelectedDecryptor.TEMod = 0
        SelectedDecryptor.RunMod = 0
        SelectedDecryptor.ProductionMod = 1
        SelectedDecryptor.Name = None

        ' For the disabling of the price update form
        PriceCheckT1Enabled = True
        PriceCheckT2Enabled = True
        PriceCheckT3Enabled = True
        PriceCheckT4Enabled = True
        PriceCheckT5Enabled = True
        PriceCheckT6Enabled = True

        ' Tool Tips
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblBPInventionCost, "Invention Cost for Runs entered = (Datacores + Decryptors) / Invented Runs * Runs (based on the probability of success)" & vbCrLf & "Double-Click for material list needed for enough successful BPCs for runs entered")
            ttBP.SetToolTip(lblBPRECost, "Invention Cost for Runs entered = (Datacores + Decryptors + Relics) / Invented Runs * Runs (based on the probability of success)" & vbCrLf & "Double-Click for material list needed for enough successful BPCs for runs entered")
            ttBP.SetToolTip(lblBPCopyCosts, "Total Cost of materials to make enough BPCs for the number of invention jobs needed" & vbCrLf & "Double-Click for material list needed for enough successful BPCs for runs entered")
            ttBP.SetToolTip(lblBPRuns, "Total number of items to produce. I.e. If you have 5 blueprints with 4 runs each, then enter 20")
            ttBP.SetToolTip(lblBPTaxes, "Sales Taxes to set up a sell order at an NPC Station")
            ttBP.SetToolTip(lblBPBrokerFees, "Broker's Fees to set up a sell order at an NPC Station")
            ttBP.SetToolTip(lblBPTotalCompCost, "Total Cost of Component Materials, InventionCosts, Usage, Taxes and Fees - Double Click for list of costs")
            ttBP.SetToolTip(lblBPRawTotalCost, "Total Cost of Raw Materials, InventionCosts, Usage, Taxes and Fees - Double Click for list of costs")
            ttBP.SetToolTip(lblBPPT, "This is the time to build the item (including skill and implant modifiers) from the blueprint after all materials are gathered")
            ttBP.SetToolTip(lblBPCPTPT, "This is the total time to build the item and components and if selected, time to complete invention and copying")
            ttBP.SetToolTip(lblBPCanMakeBP, "Double-Click here to see required skills to make this BP")
            ttBP.SetToolTip(lblBPCanMakeBPAll, "Double-Click here to see required skills to make all the items for this BP")
            ttBP.SetToolTip(lblBPT2InventStatus, "Double-Click here to see required skills to invent this BP")
            ttBP.SetToolTip(lblT3InventStatus, "Double-Click here to see required skills to invent this BP")
            ttBP.SetToolTip(chkBPPricePerUnit, "Show Price per Unit - All price data in this frame will be updated to show the prices for 1 unit")
            ttBP.SetToolTip(lblBPProductionTime, "Total time to build this blueprint with listed components")
            ttBP.SetToolTip(lblBPTotalItemPT, "Total time to build selected build components and this blueprint")
            ttBP.SetToolTip(lblBPComponentMats, "Total list of components, which can be built, and materials to build this blueprint")
            ttBP.SetToolTip(lblBPRawMats, "Total list of materials to build all components and base materials for this blueprint")
            ttBP.SetToolTip(lblBPDecryptorStats, "Selected Decryptor Stats and Runs per BPC")
            ttBP.SetToolTip(lblBPT3Stats, "Selected Decryptor Stats and Runs per BPC")
            ttBP.SetToolTip(lblBPSimpleCopy, "When checked, this will copy the list into a format that will work with Multi-Buy when pressing the Copy button.")
            ttBP.SetToolTip(lblBPRawProfit, "Double-Click to toggle value between Profit and Profit Percent")
            ttBP.SetToolTip(lblBPCompProfit, "Double-Click to toggle value between Profit and Profit Percent")
        End If

        '*******************************************
        '**** Update Prices Tab Initializations ****
        '*******************************************
        ' Create the controls collection class
        m_ControlsCollection = New ControlsCollection(Me)
        ' Get Region check boxes (note index starts at 1)
        RegionCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkRegion"), CheckBox())
        TechCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkPricesT"), CheckBox())
        SystemCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkSystems"), CheckBox())

        ' Columns of Update Prices Listview (width = 639) + 21 for scroll = 660
        lstPricesView.Columns.Add("TypeID", 0, HorizontalAlignment.Left) ' Hidden
        lstPricesView.Columns.Add("Group", 220, HorizontalAlignment.Left)
        lstPricesView.Columns.Add("Item", 319, HorizontalAlignment.Left)
        lstPricesView.Columns.Add("Price", 100, HorizontalAlignment.Right)
        lstPricesView.Columns.Add("Manufacture", 0, HorizontalAlignment.Right) ' Hidden
        lstPricesView.Columns.Add("Market ID", 0, HorizontalAlignment.Right) ' Hidden
        lstPricesView.Columns.Add("Price Type", 0, HorizontalAlignment.Right) ' Hidden

        ' Columns of update prices raw mats in price profiles
        lstRawPriceProfile.Columns.Add("Group", 136, HorizontalAlignment.Left)
        lstRawPriceProfile.Columns.Add("Price Type", 80, HorizontalAlignment.Left)
        lstRawPriceProfile.Columns.Add("Region", 98, HorizontalAlignment.Left) ' 119 is to fit all regions
        lstRawPriceProfile.Columns.Add("Solar System", 84, HorizontalAlignment.Left) ' 104 is to fit all systems
        lstRawPriceProfile.Columns.Add("PMod", 41, HorizontalAlignment.Right) 'Hidden

        ' Columns of update prices manufactured mats in price profiles
        lstManufacturedPriceProfile.Columns.Add("Group", 136, HorizontalAlignment.Left)
        lstManufacturedPriceProfile.Columns.Add("Price Type", 80, HorizontalAlignment.Left)
        lstManufacturedPriceProfile.Columns.Add("Region", 98, HorizontalAlignment.Left) ' 119 is to fit all regions
        lstManufacturedPriceProfile.Columns.Add("Solar System", 84, HorizontalAlignment.Left) ' 104 is to fit all systems
        lstManufacturedPriceProfile.Columns.Add("PMod", 41, HorizontalAlignment.Right) ' Hidden

        ' Tool Tips
        If UserApplicationSettings.ShowToolTips Then
            ttUpdatePrices.SetToolTip(cmbRawMatsSplitPrices, "Buy = Use Buy orders only" & vbCrLf &
                                                            "Sell = Use Sell orders only" & vbCrLf &
                                                            "Buy & Sell = Use All orders" & vbCrLf &
                                                            "Min = Minimum" & vbCrLf &
                                                            "Max = Maximum" & vbCrLf &
                                                            "Avg = Average" & vbCrLf &
                                                            "Med = Median" & vbCrLf &
                                                            "Percentile = 5% of the top prices (Buy) or bottom (Sell, All) ")
            ttUpdatePrices.SetToolTip(cmbItemsSplitPrices, "Buy = Use Buy orders only" & vbCrLf &
                                                            "Sell = Use Sell orders only" & vbCrLf &
                                                            "Buy & Sell = Use All orders" & vbCrLf &
                                                            "Min = Minimum" & vbCrLf &
                                                            "Max = Maximum" & vbCrLf &
                                                            "Avg = Average" & vbCrLf &
                                                            "Med = Median" & vbCrLf &
                                                            "Percentile = 5% of the top prices (Buy) or bottom (Sell, All) ")
        End If

        FirstSolarSystemComboLoad = True
        FirstPriceChargeTypesComboLoad = True
        FirstPriceShipTypesComboLoad = True
        IgnoreSystemCheckUpdates = False
        IgnoreRegionCheckUpdates = False

        PriceTypeUpdate = False
        PriceSystemUpdate = False
        PriceRegionUpdate = False
        PriceModifierUpdate = False
        PreviousPriceType = ""
        PreviousRegion = ""
        PreviousSystem = ""
        PreviousPriceMod = ""
        TabPressed = False
        UpdatingCombo = False

        PPRawSystemsLoaded = False
        PPItemsSystemsLoaded = False

        PriceHistoryUpdateCount = 0
        PriceOrdersUpdateCount = 0
        CancelUpdatePrices = False
        CancelManufacturingTabCalc = False

        Call InitUpdatePricesTab()

        '****************************************
        '**** Manufacturing Tab Initializations ****
        '****************************************
        CalcRelicCheckboxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkCalcRERelic"), CheckBox())
        CalcDecryptorCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkCalcDecryptor"), CheckBox())

        ' Add the columns based on settings
        Call RefreshManufacturingTabColumns()

        If UserApplicationSettings.ShowToolTips Then
            ' Decryptor Tool tips
            ttUpdatePrices.SetToolTip(chkCalcDecryptor2, "Augmentation - (PM: 0.6, Runs: +9, ME: -2, TE: +1)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor3, "Optimized Augmentation - (PM: 0.9, Runs: +7, ME +2 TE: 0)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor4, "Symmetry - (PM: 1.0, Runs: +2, ME: +1, TE: +4)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor5, "Process - (PM: 1.1, Runs: 0, ME: +3, TE: +3)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor6, "Accelerant - (PM: 1.2, Runs: +1, ME: +2, TE: +5)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor7, "Parity - (PM: 1.5, Runs: +3, ME: +1, TE: -1)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor8, "Attainment - (PM: 1.8, Runs: +4, ME: -1, TE: +2)")
            ttUpdatePrices.SetToolTip(chkCalcDecryptor9, "Optimized Attainment - (PM: 1.9, Runs: +2, ME: +1, TE: -1)")

            ttUpdatePrices.SetToolTip(txtCalcProdLines, "Will assume Number of BPs is same as Number of Production lines for Calculations")
            ttUpdatePrices.SetToolTip(chkCalcTaxes, "Sales Taxes to set up a sell order at an NPC Station for the Item")
            ttUpdatePrices.SetToolTip(chkCalcFees, "Broker's Fees to set up a sell order at an NPC Station for the Item")

            ttUpdatePrices.SetToolTip(txtCalcProdLines, "Enter the number of Manufacturing Lines you have to build items per day for calculations. Calculations will assume the same number of BPs used." & vbCrLf & "Calculations for components will also use this value. Double-Click to enter max runs for this character.")
            ttUpdatePrices.SetToolTip(txtCalcLabLines, "Enter the number of Laboratory Lines you have to invent per day for calculations. Double-Click to enter max runs for this character.")

            ttUpdatePrices.SetToolTip(txtCalcSVRThreshold, "No results with an SVR lower than the number entered will be returned.")

        End If
        FirstLoadCalcBPTypes = True
        FirstManufacturingGridLoad = True

        ' If there is an error in price updates, only show once
        ShownPriceUpdateError = False

        Call InitManufacturingTab()

        '****************************************
        '**** Datacores Tab Initializations *****
        '****************************************
        DCSkillCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkDC"), CheckBox())
        DCSkillLabels = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "lblDatacore"), Label())
        DCSkillCombos = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "cmbDCSkillLevel"), ComboBox())
        DCCorpCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkDCCorp"), CheckBox())
        DCCorpLabels = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "lblDCCorp"), Label())
        DCCorpTextboxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "txtDCStanding"), TextBox())

        FirstShowDatacores = True
        DCRegionsLoaded = False
        rbtnDCUpdatedPrices.Checked = True
        TotalSelectedIPH = 0

        txtDCTotalOptIPH.Text = "0.00"
        txtDCTotalSelectedIPH.Text = "0.00"

        ' Width 1124, 21 for scrollbar, 25 for check
        lstDC.Columns.Add("", -2, HorizontalAlignment.Center) ' For check
        lstDC.Columns.Add("Corporation", 120, HorizontalAlignment.Left)
        lstDC.Columns.Add("Agent", 152, HorizontalAlignment.Left)
        lstDC.Columns.Add("LVL", 40, HorizontalAlignment.Center)
        lstDC.Columns.Add("Standing", 60, HorizontalAlignment.Right)
        lstDC.Columns.Add("Location", 250, HorizontalAlignment.Left) ' System name and security (station name?)
        lstDC.Columns.Add("DataCore Skill", 166, HorizontalAlignment.Left)
        lstDC.Columns.Add("DataCore Price", 88, HorizontalAlignment.Right)
        lstDC.Columns.Add("Price From", 65, HorizontalAlignment.Center) ' Load with system name, region, or multiple
        lstDC.Columns.Add("Core/Day", 62, HorizontalAlignment.Right)
        lstDC.Columns.Add("Isk per Hour", 75, HorizontalAlignment.Right)
        DCIPH_COLUMN = 10 ' For totaling up the price

        If UserApplicationSettings.ShowToolTips Then
            ttDatacores.SetToolTip(rbtnDCSystemPrices, "Max Buy Order from System used for Datacore Price")
            ttDatacores.SetToolTip(rbtnDCRegionPrices, "Max Buy Order from Region used for Datacore Price")
            ttDatacores.SetToolTip(lblDCGreenBackColor, "Green Background: Max IPH Agent")
            ttDatacores.SetToolTip(lblDCBlueText, "Blue Text: Current Research Agents")
            ttDatacores.SetToolTip(lblDCGrayText, "Gray Text: Unavailable Research Agent")
            ttDatacores.SetToolTip(lblDCOrangeText, "Orange Text: Research Agent is in Low Sec")
            ttDatacores.SetToolTip(lblDCRedText, "Red Text: Research Agent is in Null Sec")
        End If

        '****************************************
        '**** Reactions Tab Initializations *****
        '****************************************
        ' 922 width, 21 for scroll
        lstReactions.Columns.Add("Reaction Type", 136, HorizontalAlignment.Left)
        lstReactions.Columns.Add("Reaction", 210, HorizontalAlignment.Left)
        lstReactions.Columns.Add("Output Material", 222, HorizontalAlignment.Left)
        lstReactions.Columns.Add("Output Quantity", 100, HorizontalAlignment.Right)
        lstReactions.Columns.Add("Material Group", 118, HorizontalAlignment.Left)
        lstReactions.Columns.Add("Isk per Hour", 115, HorizontalAlignment.Right)

        Call InitReactionsTab()

        ' Tool Tips
        If UserApplicationSettings.ShowToolTips Then
            ttReactions.SetToolTip(chkReactionsTaxes, "Include taxes charged for sale of Reaction Products")
            ttReactions.SetToolTip(chkReactionsFees, "Include Broker Fees charged for placing a buy order for Reaction Products")
        End If

        ' Set up grid for input mats
        lstReactionMats.Columns.Add("Material", 117, HorizontalAlignment.Left)
        'lstReactionMats.Columns.Add("Cost", 50, HorizontalAlignment.Right)
        lstReactionMats.Columns.Add("Quantity", 52, HorizontalAlignment.Right)

        '****************************************
        '**** Mining Tab Initializations ********
        '****************************************
        lstMineGrid.Columns.Add("Ore ID", 0, HorizontalAlignment.Right) ' Hidden
        lstMineGrid.Columns.Add("Ore Name", MineOreNameColumnWidth, HorizontalAlignment.Left)
        lstMineGrid.Columns.Add("Refine Type", 70, HorizontalAlignment.Left)
        lstMineGrid.Columns.Add("Unit Price", 100, HorizontalAlignment.Right)
        lstMineGrid.Columns.Add("Refine Yield", MineRefineYieldColumnWidth, HorizontalAlignment.Center)
        lstMineGrid.Columns.Add("Crystal", MineCrystalColumnWidth, HorizontalAlignment.Left)
        lstMineGrid.Columns.Add("m3 per Cycle", 75, HorizontalAlignment.Right)
        lstMineGrid.Columns.Add("Units per Hour", 94, HorizontalAlignment.Right)
        lstMineGrid.Columns.Add("Isk per Hour", 105, HorizontalAlignment.Right)

        MineProcessingCheckBoxes = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "chkOreProcessing"), CheckBox())
        MineProcessingLabels = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "lblOreProcessing"), Label())
        MineProcessingCombos = DirectCast(ControlArrayUtils.getControlArray(Me, Me.MyControls, "cmbOreProcessing"), ComboBox())

        Call InitMiningTab()

        ' Tool Tips
        If UserApplicationSettings.ShowToolTips Then
            ttMining.SetToolTip(rbtnMineT2Crystals, "Use T2 Crystals when skills and equipment allow")
            ttMining.SetToolTip(gbMineHauling, "If no hauling, results will take into account Round Trip time to the station based on M3 of Ship and fill times")
            ttMining.SetToolTip(btnMineSaveAllSettings, "Saves all current options on Mining Screen")
            ttMining.SetToolTip(chkMineForemanLaserOpBoost, "Click to cycle through No Booster, T1 or T2")
            ttMining.SetToolTip(chkMineForemanLaserRangeBoost, "Click to cycle through No Booster, T1 or T2")
            ttMining.SetToolTip(cmbMineIndustReconfig, "Select skill level to include Heavy Water costs per hour, set to 0 to ignore")
            ttMining.SetToolTip(chkMineRorqDeployedMode, "To include Heavy Water costs for deployed mode, select Industrial Reconfiguration skill other than 0")
            ttMining.SetToolTip(lblMineExhumers, "For Prospect Mining Frigate, use the Exhumers Combobox to set the Expedition Frigate skill level.")
        End If

        '****************************************
        '**** All Tabs **************************
        '****************************************

        ' For indy jobs viewer
        FirstIndustryJobsViewerLoad = True

        ' For handling click events
        UpdatingCheck = False

        ' All set, we are done loading
        FirstLoad = False

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        On Error Resume Next
        EVEDB.CloseDB()
        On Error GoTo 0
    End Sub

    Public ReadOnly Property MyControls() As Collection
        Get
            Return m_ControlsCollection.Controls
        End Get
    End Property

    ' Inits the Invention tab so it shows correctly with themes
    Public Sub InitInventionTab()
        Dim sb As String = String.Empty
        Dim v As String = String.Empty

        On Error Resume Next
        SetWindowTheme(Me.tabBPInventionEquip.Handle, " ", " ")
        On Error GoTo 0

    End Sub

#End Region

#Region "Form Functions/Procedures"

#Region "Tool Tip Processing"
    ' Datacore Tab
    Private Sub lblDCGreenBackColor_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblDCGreenBackColor.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblDCGreenBackColor, "Green Background: Max IPH Agent")
        End If
    End Sub

    Private Sub lblDCBlueText_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblDCBlueText.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblDCBlueText, "Blue Text: Current Research Agents")
        End If
    End Sub

    Private Sub lblDCGrayText_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblDCGrayText.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblDCGrayText, "Gray Text: Unavailable Research Agent")
        End If
    End Sub

    Private Sub lblDCOrangeText_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblDCOrangeText.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblDCOrangeText, "Orange Text: Research Agent is in Low Sec")
        End If
    End Sub

    Private Sub lblDCRedText_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblDCRedText.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblDCRedText, "Red Text: Research Agent is in Null Sec")
        End If
    End Sub

    Private Sub rbtnDCSystemPrices_MouseEnter(sender As System.Object, e As System.EventArgs) Handles rbtnDCSystemPrices.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(rbtnDCSystemPrices, "Max Buy Order from System used for Datacore Price")
        End If
    End Sub

    Private Sub rbtnDCRegionPrices_MouseEnter(sender As System.Object, e As System.EventArgs) Handles rbtnDCRegionPrices.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(rbtnDCRegionPrices, "Max Buy Order from Region used for Datacore Price")
        End If
    End Sub

    ' Manufacturing Tab
    Private Sub lblCalcColorCode1_MouseEnter(sender As Object, e As System.EventArgs) Handles lblCalcColorCode1.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblCalcColorCode1, "Beige Background: Owned Blueprint")
        End If
    End Sub

    Private Sub lblCalcColorCode2_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblCalcColorCode2.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblCalcColorCode2, "Light Blue Background: T2 item with Owned T1 Blueprint (for invention)")
        End If
    End Sub

    Private Sub lblCalcColorCode3_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblCalcColorCode3.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblCalcColorCode5, "Green Text: Unable to T3 Invent Item")
        End If
    End Sub

    Private Sub lblCalcColorCode4_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblCalcColorCode4.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblCalcColorCode4, "Orange Text: Unable to Invent Item")
        End If
    End Sub

    Private Sub lblCalcColorCode5_MouseEnter(sender As System.Object, e As System.EventArgs) Handles lblCalcColorCode5.MouseEnter
        If UserApplicationSettings.ShowToolTips Then
            ttBP.SetToolTip(lblCalcColorCode3, "Red Text: Unable to Build Item")
        End If
    End Sub

#End Region

#Region "SVR Functions"

    ' Determine the Sales per Volume Ratio, which will be the amount of items we can build in 24 hours (include fractions) when sent the region, avg days, and production time in seconds to make ItemsProduced (runs * portion size)
    Public Function GetItemSVR(TypeID As Long, RegionID As Long, AvgDays As Integer, ProductionTimeSeconds As Double,
                               ItemsProduced As Long) As String
        Dim SQL As String
        Dim readerAverage As SQLiteDataReader
        Dim ItemsperDay As Double

        ' The amount of items we can build in 24 hours (include fractions) divided by the average volume (volume/avgdays)
        ' The data is stored as a record per day, so just count up the number of records in the time period (days - might not be the same as days shown)
        ' and divide by the sum of the volumes over that time period
        SQL = "SELECT SUM(TOTAL_VOLUME_FILLED)/COUNT(PRICE_HISTORY_DATE) FROM MARKET_HISTORY "
        SQL = SQL & "WHERE TYPE_ID = " & TypeID & " AND REGION_ID = " & RegionID & " "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) >= " & " DateTime('" & Format(DateAdd(DateInterval.Day, -(AvgDays + 1), Now.Date), SQLiteDateFormat) & "') "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) < " & " DateTime('" & Format(Now.Date, SQLiteDateFormat) & "') "
        SQL = SQL & "AND TOTAL_VOLUME_FILLED IS NOT NULL "

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerAverage = DBCommand.ExecuteReader

        readerAverage.Read()

        If Not IsDBNull(readerAverage.GetValue(0)) Then
            If ProductionTimeSeconds <> 0 Then
                ' The number of blueprint runs we can build with the sent production time in a day - Seconds to produce 1, then divide that into seconds per day
                ItemsperDay = (24 * 60 * 60) / (ProductionTimeSeconds / ItemsProduced)
                ' Take the items per day and compare to the avg sales volume per day, if it's greater than one you can't make enough items in a day to meet demand = good item to build
                Return FormatNumber(readerAverage.GetDouble(0) / ItemsperDay, 2)
            Else
                ' Just want the volume for the day
                Return FormatNumber(readerAverage.GetDouble(0), 2)
            End If
        Else
            ' Since 0.00 SVR is possible, return nothing instead
            Return "-"
        End If

    End Function

    ' Updates the SVR value and then returns it as a string for the item associated with the Selected BP
    Private Function GetBPItemSVR(ProductionTime As Double) As String
        Dim MH As New MarketPriceInterface(Nothing)

        Application.UseWaitCursor = True
        Application.DoEvents()
        Dim TypeID As New List(Of Long) ' for just one
        Dim RegionID As Long = GetRegionID(UserApplicationSettings.SVRAveragePriceRegion)

        Call TypeID.Add(SelectedBlueprint.GetItemID)
        PriceHistoryUpdateCount = 0
        If Not MH.UpdateESIPriceHistory(TypeID, RegionID) Then
            Call MsgBox("Some prices did not update. Please try again.", vbInformation, Application.ProductName)
        End If

        Dim ReturnValue As String = GetItemSVR(TypeID(0), RegionID, CInt(UserApplicationSettings.SVRAveragePriceDuration), ProductionTime,
                                                SelectedBlueprint.GetTotalUnits)

        Application.UseWaitCursor = False
        Application.DoEvents()

        Return ReturnValue

    End Function

#End Region

    Public Sub LoadCharacterNamesinMenu()
        ' Default character set, now set the menu name on the panel
        mnuCharacter.Text = "Character Loaded: " & SelectedCharacter.Name
        ' Also, load all characters we have
        Dim rsCharacters As SQLiteDataReader
        Dim SQL As String = "SELECT CHARACTER_NAME, CASE WHEN GENDER IS NULL THEN 'male' ELSE GENDER END AS GENDER "
        SQL = SQL & "FROM ESI_CHARACTER_DATA ORDER BY CHARACTER_NAME"
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsCharacters = DBCommand.ExecuteReader

        Dim Counter As Integer = 0

        ' Reset all 
        tsCharacter1.Text = ""
        tsCharacter1.Visible = False
        tsCharacter2.Text = ""
        tsCharacter2.Visible = False
        tsCharacter3.Text = ""
        tsCharacter3.Visible = False
        tsCharacter4.Text = ""
        tsCharacter4.Visible = False
        tsCharacter5.Text = ""
        tsCharacter5.Visible = False
        tsCharacter6.Text = ""
        tsCharacter6.Visible = False
        tsCharacter7.Text = ""
        tsCharacter7.Visible = False
        tsCharacter8.Text = ""
        tsCharacter8.Visible = False
        tsCharacter9.Text = ""
        tsCharacter9.Visible = False
        tsCharacter10.Text = ""
        tsCharacter10.Visible = False
        tsCharacter11.Text = ""
        tsCharacter11.Visible = False
        tsCharacter12.Text = ""
        tsCharacter12.Visible = False
        tsCharacter13.Text = ""
        tsCharacter13.Visible = False
        tsCharacter14.Text = ""
        tsCharacter14.Visible = False
        tsCharacter15.Text = ""
        tsCharacter15.Visible = False
        tsCharacter16.Text = ""
        tsCharacter16.Visible = False
        tsCharacter17.Text = ""
        tsCharacter17.Visible = False
        tsCharacter18.Text = ""
        tsCharacter18.Visible = False
        tsCharacter19.Text = ""
        tsCharacter19.Visible = False
        tsCharacter20.Text = ""
        tsCharacter20.Visible = False

        While rsCharacters.Read
            ' Add all the character names to the list for the number we have - only load 20 characters
            Select Case Counter
                Case 0
                    tsCharacter1.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter1, rsCharacters.GetString(1))
                    tsCharacter1.Visible = True
                Case 1
                    tsCharacter2.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter2, rsCharacters.GetString(1))
                    tsCharacter2.Visible = True
                Case 2
                    tsCharacter3.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter3, rsCharacters.GetString(1))
                    tsCharacter3.Visible = True
                Case 3
                    tsCharacter4.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter4, rsCharacters.GetString(1))
                    tsCharacter4.Visible = True
                Case 4
                    tsCharacter5.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter5, rsCharacters.GetString(1))
                    tsCharacter5.Visible = True
                Case 5
                    tsCharacter6.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter6, rsCharacters.GetString(1))
                    tsCharacter6.Visible = True
                Case 6
                    tsCharacter7.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter7, rsCharacters.GetString(1))
                    tsCharacter7.Visible = True
                Case 7
                    tsCharacter8.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter8, rsCharacters.GetString(1))
                    tsCharacter8.Visible = True
                Case 8
                    tsCharacter9.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter9, rsCharacters.GetString(1))
                    tsCharacter9.Visible = True
                Case 9
                    tsCharacter10.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter10, rsCharacters.GetString(1))
                    tsCharacter10.Visible = True
                Case 10
                    tsCharacter11.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter11, rsCharacters.GetString(1))
                    tsCharacter11.Visible = True
                Case 11
                    tsCharacter12.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter12, rsCharacters.GetString(1))
                    tsCharacter12.Visible = True
                Case 12
                    tsCharacter13.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter13, rsCharacters.GetString(1))
                    tsCharacter13.Visible = True
                Case 13
                    tsCharacter14.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter14, rsCharacters.GetString(1))
                    tsCharacter14.Visible = True
                Case 14
                    tsCharacter15.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter15, rsCharacters.GetString(1))
                    tsCharacter15.Visible = True
                Case 15
                    tsCharacter16.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter16, rsCharacters.GetString(1))
                    tsCharacter16.Visible = True
                Case 16
                    tsCharacter17.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter17, rsCharacters.GetString(1))
                    tsCharacter17.Visible = True
                Case 17
                    tsCharacter18.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter18, rsCharacters.GetString(1))
                    tsCharacter18.Visible = True
                Case 18
                    tsCharacter19.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter19, rsCharacters.GetString(1))
                    tsCharacter19.Visible = True
                Case 19
                    tsCharacter20.Text = rsCharacters.GetString(0)
                    Call SetCharToolStripImage(tsCharacter20, rsCharacters.GetString(1))
                    tsCharacter20.Visible = True
            End Select
            Counter += 1 ' increment
        End While

    End Sub

    Private Sub SetCharToolStripImage(ByRef TS As ToolStripMenuItem, ByVal Gender As String)
        If Gender = Male Then
            TS.Image = My.Resources._46_64_1
        Else
            TS.Image = My.Resources._46_64_2
        End If
        TS.ImageTransparentColor = Color.White
    End Sub

    ' Set all the tool strips for characters since I can't process them if they aren't set at runtime
    Private Sub ToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter1.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter1.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter1.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem2_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter2.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter2.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter2.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem3_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter3.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter3.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter3.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem4_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter4.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter4.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter4.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem5_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter5.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter5.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter5.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem6_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter6.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter6.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter6.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem7_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter7.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter7.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter7.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem8_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter8.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter8.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter8.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem9_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter9.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter9.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter9.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem10_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter10.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter10.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter10.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem11_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter11.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter11.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter11.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem12_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter12.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter12.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter12.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem13_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter13.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter13.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter13.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem14_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter14.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter14.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter14.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem15_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter15.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter15.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter15.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem16_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter16.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter16.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter16.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem17_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter17.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter17.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter17.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem18_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter18.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter18.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter18.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem19_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter19.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter19.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter19.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub ToolStripMenuItem20_Click(sender As System.Object, e As System.EventArgs) Handles tsCharacter20.Click
        Me.Cursor = Cursors.WaitCursor
        Call LoadSelectedCharacter(tsCharacter20.Text)
        mnuCharacter.Text = "Character Loaded: " & tsCharacter20.Text
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' After initializing everything, refresh the tabs so they draw fast on first click
        Dim temptab As TabPage
        For Each temptab In tabMain.TabPages
            tabMain.SelectTab(temptab.Name)
            tabMain.SelectedTab.Refresh()
        Next

        ' Reset to bp tab
        tabMain.SelectTab(0)

        ' Add a mouse down handler for the blueprint tab to enable forward and back loading of bps from mouse
        AddHandler tabBlueprints.MouseDown, AddressOf MouseDownHandling
        MouseDownSetting(tabBlueprints)

        ' Done loading
        Call SetProgress("")

    End Sub

    ' Adds mouse down handlers for all controls of the parent
    Private Sub MouseDownSetting(ByVal parentCtr As Control)
        Dim ctr As Control

        For Each ctr In parentCtr.Controls
            AddHandler ctr.MouseDown, AddressOf MouseDownHandling
            MouseDownSetting(ctr)
        Next

    End Sub

    ' Function to deal with mouse down events to load next or previous blueprint
    Private Sub MouseDownHandling(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If e.Button = Windows.Forms.MouseButtons.XButton1 Then
            Call LoadPreviousBlueprint()
        ElseIf e.Button = Windows.Forms.MouseButtons.XButton2 Then
            Call LoadNextBlueprint()
        End If
    End Sub

    ' Loads a BP sent from a double click on shopping list or manufacturing list, or loading history
    Public Sub LoadBPfromEvent(BPID As Long, BuildType As String, Inputs As String, SentFrom As SentFromLocation,
                                BuildFacility As IndustryFacility, ComponentFacility As IndustryFacility, CapCompentFacility As IndustryFacility,
                                InventionFacility As IndustryFacility, CopyFacility As IndustryFacility,
                                IncludeTaxes As Boolean, IncludeFees As Boolean,
                                MEValue As String, TEValue As String, SentRuns As String,
                                ManufacturingLines As String, LaboratoryLines As String, NumBPs As String,
                                AddlCosts As String, PPUCheck As Boolean,
                                Optional CompareType As String = "")
        Dim BPTech As Integer
        Dim CurrentBPCategoryID As Integer
        Dim CurrentBPGroupID As Integer
        Dim BPHasComponents As Boolean
        Dim DecryptorName As String = None
        Dim BPDecryptor As New Decryptor
        Dim readerBP As SQLiteDataReader
        Dim readerRelic As SQLiteDataReader
        Dim SQL As String

        Dim TempLines As Integer = CInt(ManufacturingLines)

        SQL = "SELECT BLUEPRINT_NAME, TECH_LEVEL, ITEM_GROUP_ID, ITEM_CATEGORY_ID FROM ALL_BLUEPRINTS WHERE BLUEPRINT_ID = " & BPID

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerBP = DBCommand.ExecuteReader

        readerBP.Read()
        RemoveHandler cmbBPBlueprintSelection.TextChanged, AddressOf cmbBPBlueprintSelection_TextChanged
        cmbBPBlueprintSelection.Text = readerBP.GetString(0)
        SelectedBPText = readerBP.GetString(0)
        AddHandler cmbBPBlueprintSelection.TextChanged, AddressOf cmbBPBlueprintSelection_TextChanged
        BPTech = readerBP.GetInt32(1)

        If BPTech = BPTechLevel.T2 Or BPTech = BPTechLevel.T3 Then
            ' Set the decryptor
            If Inputs <> None Then
                If Not CBool(InStr(Inputs, "No Decryptor")) Then
                    If BPTech = 2 Then
                        DecryptorName = Inputs
                    ElseIf InStr(Inputs, "|") <> 0 Then ' For T3
                        DecryptorName = Inputs.Substring(0, InStr(Inputs, "|") - 1)
                    ElseIf InStr(Inputs, " - ") <> 0 Then
                        DecryptorName = Inputs.Substring(0, InStr(Inputs, "-") - 2)
                    End If
                End If
            End If

            BPDecryptor = SelectDecryptor(DecryptorName)

            If BPTech = 3 Then
                LoadingT3Decryptors = True
                cmbBPT3Decryptor.Text = BPDecryptor.Name
                LoadingT3Decryptors = False

                ' Also load the relic
                LoadingRelics = True
                Dim TempRelic As String = ""
                If InStr(Inputs, "|") <> 0 Then ' For T3
                    TempRelic = Inputs.Substring(InStr(Inputs, "|")) ' Removed the -1 in the substring it was including | in the SQL Query
                ElseIf InStr(Inputs, " - ") <> 0 Then
                    TempRelic = Inputs.Substring(InStr(Inputs, "-") + 1)
                End If

                SQL = "SELECT typeName FROM INVENTORY_TYPES, INDUSTRY_ACTIVITY_PRODUCTS WHERE productTypeID =" & BPID & " "
                SQL = SQL & "AND typeID = blueprintTypeID AND activityID = 8 AND typeName LIKE '%" & TempRelic & "%'"

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerRelic = DBCommand.ExecuteReader
                readerRelic.Read()
                cmbBPRelic.Items.Clear()
                cmbBPRelic.Text = readerRelic.GetString(0)
                LoadingRelics = False
                RelicsLoaded = False ' Allow reload on drop down

            Else
                LoadingInventionDecryptors = True
                cmbBPInventionDecryptor.Text = BPDecryptor.Name
                LoadingInventionDecryptors = False
            End If

            ' Need to calculate the number of bps based on the bp
            If chkCalcAutoCalcT2NumBPs.Checked Then
                txtBPNumBPs.Text = CStr(GetUsedNumBPs(BPID, BPTech, CInt(SentRuns), TempLines, CInt(NumBPs), BPDecryptor.RunMod))
            End If
        Else
            ' T1
            txtBPNumBPs.Text = NumBPs
        End If

        ' We need to set each facility individually for later use
        BPTabFacility.UpdateFacility(CType(BuildFacility.Clone, IndustryFacility))
        BPTabFacility.UpdateFacility(CType(ComponentFacility.Clone, IndustryFacility))
        BPTabFacility.UpdateFacility(CType(CapCompentFacility.Clone, IndustryFacility))
        If BPTech = BPTechLevel.T2 Or BPTech = 3 Then
            BPTabFacility.UpdateFacility(CType(InventionFacility.Clone, IndustryFacility))
        End If
        If BPTech = 2 Then
            BPTabFacility.UpdateFacility(CType(CopyFacility.Clone, IndustryFacility))
        End If

        ' Reset the current bp selection to override what is there
        CurrentBPCategoryID = readerBP.GetInt32(3)
        CurrentBPGroupID = readerBP.GetInt32(2)
        BPHasComponents = DoesBPHaveBuildableComponents(BPID)

        ' Common to all settings
        If CompareType <> "" Then ' if "" then let the BP tab handle it
            If CompareType = "Components" Then
                rbtnBPComponentCopy.Checked = True
            Else
                rbtnBPRawmatCopy.Checked = True
            End If
        End If

        chkBPTaxes.Checked = IncludeTaxes
        chkBPBrokerFees.Checked = IncludeFees

        txtBPLines.Text = ManufacturingLines
        txtBPInventionLines.Text = LaboratoryLines
        txtBPRelicLines.Text = LaboratoryLines
        txtBPRuns.Text = SentRuns

        txtBPAddlCosts.Text = AddlCosts
        txtBPME.Text = MEValue
        txtBPTE.Text = TEValue
        UpdatingCheck = True
        chkBPPricePerUnit.Checked = PPUCheck
        UpdatingCheck = False

        ' Set the optimize check
        UpdatingCheck = True
        If BuildType = "Build/Buy" Then
            chkBPBuildBuy.Checked = True
        Else
            chkBPBuildBuy.Checked = False
        End If
        UpdatingCheck = False

        ' Show the BP tab 
        tabMain.SelectedTab = tabBlueprints
        readerBP.Close()

        ' Finally, load the blueprint with data in the row selected like it was just selected
        Call SelectBlueprint(True, SentFrom)

    End Sub

    ' Loads the popup with all the material break down and usage for invention
    Private Sub lblBPInventionCost_DoubleClick(sender As System.Object, e As System.EventArgs) Handles lblBPInventionCost.DoubleClick
        Dim f1 As New frmInventionMats

        Me.Cursor = Cursors.WaitCursor

        If Not IsNothing(SelectedBlueprint) Then
            If SelectedBlueprint.GetTechLevel = BPTechLevel.T2 Then
                f1.MatType = "T2 Invention Materials needed for enough successful BPCs for " & CStr(SelectedBlueprint.GetUserRuns)
                If SelectedBlueprint.GetUserRuns = 1 Then
                    f1.MatType = f1.MatType & " Run"
                Else
                    f1.MatType = f1.MatType & " Runs"
                End If
                f1.MaterialList = SelectedBlueprint.GetInventionMaterials
                f1.TotalInventedRuns = SelectedBlueprint.GetTotalInventedRuns
                f1.UserRuns = SelectedBlueprint.GetUserRuns
                f1.ListType = "Invention"
            End If
            Me.Cursor = Cursors.Default
            f1.Show()
        End If

    End Sub

    ' Loads the popup with all the copy materials and usage for copy jobs
    Private Sub lblBPCopyCosts_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPCopyCosts.DoubleClick
        Dim f1 As New frmInventionMats

        If Not IsNothing(SelectedBlueprint) Then
            If SelectedBlueprint.GetTechLevel = BPTechLevel.T2 Then
                f1.MatType = "T2 Copy Materials needed for enough successful BPCs for " & CStr(SelectedBlueprint.GetUserRuns)
                If SelectedBlueprint.GetUserRuns = 1 Then
                    f1.MatType = f1.MatType & " Run"
                Else
                    f1.MatType = f1.MatType & " Runs"
                End If
                f1.MaterialList = SelectedBlueprint.GetCopyMaterials
                f1.TotalInventedRuns = SelectedBlueprint.GetInventionJobs
                f1.ListType = "Copying"
            End If
            f1.Show()
        End If

    End Sub

    ' Loads the popup with all the material break down and usage for T3 invention
    Private Sub lblBPRECost_DoubleClick(sender As System.Object, e As System.EventArgs) Handles lblBPRECost.DoubleClick
        Dim f1 As New frmInventionMats

        If Not IsNothing(SelectedBlueprint) Then
            If SelectedBlueprint.GetTechLevel = BPTechLevel.T3 Then
                f1.MatType = "T3 Invention Materials needed for enough successful BPCs for " & CStr(SelectedBlueprint.GetUserRuns)
                If SelectedBlueprint.GetUserRuns = 1 Then
                    f1.MatType = f1.MatType & " Run"
                Else
                    f1.MatType = f1.MatType & " Runs"
                End If
                f1.MaterialList = SelectedBlueprint.GetInventionMaterials
                f1.TotalInventedRuns = SelectedBlueprint.GetTotalInventedRuns
                f1.UserRuns = SelectedBlueprint.GetUserRuns
                f1.ListType = "T3 Invention"
            End If
            f1.Show()
        End If

    End Sub

    ' Loads the popup with all the costs for total raw
    Private Sub lblBPRawTotalCost_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPRawTotalCost.DoubleClick
        Call ShowCostSplitViewer(SelectedBlueprint.GetRawMaterials.GetTotalMaterialsCost, "Raw")
    End Sub

    ' Loads the popup with all the costs for total components
    Private Sub lblBPTotalCompCost_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPTotalCompCost.DoubleClick
        Call ShowCostSplitViewer(SelectedBlueprint.GetComponentMaterials.GetTotalMaterialsCost, "Component")
    End Sub

    ' Loads the popup with all the costs for the material cost sent
    Private Sub ShowCostSplitViewer(MaterialsCost As Double, MaterialType As String)
        Dim f1 As New frmCostSplitViewer
        Dim RawCostSplit As CostSplit

        ' Fill up the array to display
        If Not IsNothing(SelectedBlueprint) Then
            f1.CostSplitType = "Total " & MaterialType & " Material Cost Split"
            ' Mat cost
            RawCostSplit.SplitName = MaterialType & " Materials Cost"
            RawCostSplit.SplitValue = MaterialsCost
            f1.CostSplits.Add(RawCostSplit)

            ' Manufacturing Facility usage
            RawCostSplit.SplitName = "Manufacturing Facility Usage"
            RawCostSplit.SplitValue = SelectedBlueprint.GetManufacturingFacilityUsage
            f1.CostSplits.Add(RawCostSplit)

            If (SelectedBlueprint.HasComponents And chkBPBuildBuy.Checked = True) Or MaterialType = "Raw" Then
                ' Component Facility Usage
                RawCostSplit.SplitName = "Component Facility Usage"
                RawCostSplit.SplitValue = SelectedBlueprint.GetComponentFacilityUsage
                f1.CostSplits.Add(RawCostSplit)

                ' Capital Component Facility Usage
                Select Case SelectedBlueprint.GetItemGroupID
                    Case ItemIDs.TitanGroupID, ItemIDs.SupercarrierGroupID, ItemIDs.CarrierGroupID, ItemIDs.DreadnoughtGroupID,
                        ItemIDs.JumpFreighterGroupID, ItemIDs.FreighterGroupID, ItemIDs.IndustrialCommandShipGroupID, ItemIDs.CapitalIndustrialShipGroupID, ItemIDs.FAXGroupID
                        ' Only add cap component usage for ships that use them
                        RawCostSplit.SplitName = "Capital Component Facility Usage"
                        RawCostSplit.SplitValue = SelectedBlueprint.GetCapComponentFacilityUsage
                        f1.CostSplits.Add(RawCostSplit)
                End Select
            End If

            ' Taxes
            RawCostSplit.SplitName = "Taxes"
            RawCostSplit.SplitValue = SelectedBlueprint.GetSalesTaxes
            f1.CostSplits.Add(RawCostSplit)

            ' Broker fees
            RawCostSplit.SplitName = "Broker Fees"
            RawCostSplit.SplitValue = SelectedBlueprint.GetSalesBrokerFees
            f1.CostSplits.Add(RawCostSplit)

            ' Additional Costs the user added
            If SelectedBlueprint.GetAdditionalCosts <> 0 Then
                RawCostSplit.SplitName = "Additional Costs"
                RawCostSplit.SplitValue = SelectedBlueprint.GetAdditionalCosts
                f1.CostSplits.Add(RawCostSplit)
            End If

            If SelectedBlueprint.GetTechLevel <> BPTechLevel.T1 Then
                ' Total Invention Costs
                RawCostSplit.SplitName = "Invention Costs"
                RawCostSplit.SplitValue = SelectedBlueprint.GetInventionCost
                f1.CostSplits.Add(RawCostSplit)

                RawCostSplit.SplitName = "Invention Usage"
                RawCostSplit.SplitValue = SelectedBlueprint.GetInventionUsage
                f1.CostSplits.Add(RawCostSplit)

                ' Total Copy Costs
                RawCostSplit.SplitName = "Copy Costs"
                RawCostSplit.SplitValue = SelectedBlueprint.GetCopyCost
                f1.CostSplits.Add(RawCostSplit)

                RawCostSplit.SplitName = "Copy Usage"
                RawCostSplit.SplitValue = SelectedBlueprint.GetCopyUsage
                f1.CostSplits.Add(RawCostSplit)

            End If

            f1.Show()
        End If

    End Sub

    ' Opens the refinery window from menu
    Private Sub mnuRefinery_Click(sender As System.Object, e As System.EventArgs) Handles mnuRefinery.Click
        Dim f1 As New frmRefinery

        Call f1.Show()

    End Sub

    ' Clears the BP history (forward / back) functionality
    Private Sub mnuClearBPHistory_Click(sender As System.Object, e As System.EventArgs) Handles mnuClearBPHistory.Click
        BPHistory = New List(Of BPHistoryItem)
        ' Reset the index
        CurrentBPHistoryIndex = -1
        ' Save the current bp we are on
        Call UpdateBPHistory(True)
    End Sub

    ' Menu update to show the patch notes
    Private Sub mnuPatchNotes_Click(sender As System.Object, e As System.EventArgs) Handles mnuPatchNotes.Click
        Dim f1 As New frmPatchNotes

        Application.UseWaitCursor = True
        Application.DoEvents()

        f1.Show()

    End Sub

    ' Shows the Invention Success Monitor
    Private Sub mnuInventionSuccessMonitor_Click(sender As System.Object, e As System.EventArgs) Handles mnuInventionSuccessMonitor.Click
        Dim f1 As New frmInventionMonitor

        f1.Show()

    End Sub

    Private Sub mnuIndustryUpgradeBelts_Click(sender As System.Object, e As System.EventArgs) Handles mnuIndustryUpgradeBelts.Click
        Dim f1 As New frmIndustryBeltFlip

        f1.Show()
    End Sub

    Private Sub mnuLPStore_Click(sender As System.Object, e As System.EventArgs) Handles mnuLPStore.Click
        Dim f1 As New frmLPStore

        f1.Show()
    End Sub

    ' Full reset - will delete all data downloaded, updated, or otherwise set by the user
    Private Sub mnuResetAllData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuResetAllData.Click
        Dim Response As MsgBoxResult
        Dim SQL As String

        Response = MsgBox("This will reset all data for the program including ESI Tokens, Blueprints, Assets, Industry Jobs, and Price data." & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            SQL = "DELETE FROM ESI_CHARACTER_DATA"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM ESI_CORPORATION_DATA"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM CHARACTER_STANDINGS"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM CHARACTER_SKILLS"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM OWNED_BLUEPRINTS"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM ITEM_PRICES_CACHE"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM ASSETS"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM INDUSTRY_JOBS"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM CURRENT_RESEARCH_AGENTS"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "UPDATE ITEM_PRICES SET PRICE = 0"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM MARKET_HISTORY"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM MARKET_HISTORY_UPDATE_CACHE"
            EVEDB.ExecuteNonQuerySQL(SQL)

            ' Reset all the cache dates
            Call ResetESIDates()

            ' Reset ESI data
            Call ResetESIIndustryFacilities()
            Call ResetESIAdjustedMarketPrices()

            FirstLoad = True ' Temporarily just to get screen to show correctly

            Application.UseWaitCursor = False
            Application.DoEvents()

            Call SelectedCharacter.LoadDummyCharacter(True)

            MsgBox("All Data Reset", vbInformation, Application.ProductName)

            ' Need to set a default, open that form
            Dim f2 = New frmSetCharacterDefault
            f2.ShowDialog()

            Call LoadCharacterNamesinMenu()

            ' Reset the tabs
            Call ResetTabs()

            FirstLoad = False

        End If

    End Sub

    Private Sub mnuResetPriceData_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetPriceData.Click
        Dim Response As MsgBoxResult
        Dim SQL As String

        Response = MsgBox("This will reset all stored price data for this character." & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            SQL = "DELETE FROM ITEM_PRICES_CACHE"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM MARKET_HISTORY"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "DELETE FROM MARKET_HISTORY_UPDATE_CACHE"
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "UPDATE ITEM_PRICES SET PRICE = 0"
            EVEDB.ExecuteNonQuerySQL(SQL)

            Application.UseWaitCursor = False
            Application.DoEvents()

            MsgBox("Prices reset", vbInformation, Application.ProductName)

        End If

        Call UpdateProgramPrices()

    End Sub

    Private Sub mnuResetESIDates_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetESIDates.Click
        Call ResetESIDates()
    End Sub

    Private Sub ResetESIDates()
        Dim SQL As String

        ' Simple update, just set all the ESI cache dates to null
        SQL = "DELETE FROM ESI_PUBLIC_CACHE_DATES"
        EVEDB.ExecuteNonQuerySQL(SQL)

        MsgBox("ESI cache dates reset", vbInformation, Application.ProductName)

    End Sub

    Private Sub ResetESIIndustryFacilities()

        ' Need to delete all outpost data, clear out the industry facilities table and set up for a rebuild
        Call EVEDB.ExecuteNonQuerySQL("DELETE FROM INDUSTRY_FACILITIES")
        Call EVEDB.ExecuteNonQuerySQL("DELETE FROM STATION_FACILITIES WHERE OUTPOST <> 0")

        ' Simple update, just set all the ESI cache dates to null
        Call EVEDB.ExecuteNonQuerySQL("UPDATE ESI_PUBLIC_CACHE_DATES SET INDUSTRY_SYSTEMS_CACHED_UNTIL = NULL")
        Call EVEDB.ExecuteNonQuerySQL("UPDATE ESI_PUBLIC_CACHE_DATES SET INDUSTRY_FACILITIES_CACHED_UNTIL = NULL")

        MsgBox("ESI Industry Facilities reset", vbInformation, Application.ProductName)

    End Sub

    Public Sub ResetESIAdjustedMarketPrices()

        ' Simple update, just set all the data back to zero
        Call EVEDB.ExecuteNonQuerySQL("UPDATE ITEM_PRICES SET ADJUSTED_PRICE = 0, AVERAGE_PRICE = 0")
        Call EVEDB.ExecuteNonQuerySQL("UPDATE ESI_PUBLIC_CACHE_DATES SET MARKET_PRICES_CACHED_UNTIL = NULL")

        MsgBox("ESI Adjusted Market Prices reset", vbInformation, Application.ProductName)

    End Sub

    Private Sub mnuResetMarketOrders_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetMarketOrders.Click

        Application.UseWaitCursor = True
        Application.DoEvents()

        ' Simple update, just set all the data back to zero
        Call EVEDB.ExecuteNonQuerySQL("DELETE FROM MARKET_ORDERS_UPDATE_CACHE")
        Call EVEDB.ExecuteNonQuerySQL("DELETE FROM MARKET_ORDERS")

        MsgBox("Market Orders reset", vbInformation, Application.ProductName)

        Application.UseWaitCursor = False
        Application.DoEvents()
    End Sub

    Private Sub mnuResetMarketHistory_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetMarketHistory.Click

        Application.UseWaitCursor = True
        Application.DoEvents()

        ' Simple update, just set all the data back to zero
        Call EVEDB.ExecuteNonQuerySQL("DELETE FROM MARKET_HISTORY_UPDATE_CACHE")
        Call EVEDB.ExecuteNonQuerySQL("DELETE FROM MARKET_HISTORY")

        MsgBox("Market History reset", vbInformation, Application.ProductName)

        Application.UseWaitCursor = False
        Application.DoEvents()
    End Sub

    Private Sub mnuResetBlueprintData_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetBlueprintData.Click
        Dim Response As MsgBoxResult

        Response = MsgBox("This will reset all blueprints for this character" & Environment.NewLine & "deleting all scanned data and stored ME/TE values." & Environment.NewLine & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            Call ResetAllBPData()

            Application.UseWaitCursor = False
            Application.DoEvents()

        End If

    End Sub

    Private Sub mnuResetAgents_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetAgents.Click
        Dim Response As MsgBoxResult
        Dim SQL As String

        Response = MsgBox("This will reset all stored Research Agents for this character." & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            SQL = "DELETE FROM CURRENT_RESEARCH_AGENTS WHERE CHARACTER_ID =" & SelectedCharacter.ID
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "UPDATE ESI_CHARACTER_DATA SET RESEARCH_AGENTS_CACHE_DATE = NULL WHERE CHARACTER_ID = " & CStr(SelectedCharacter.ID)
            EVEDB.ExecuteNonQuerySQL(SQL)

            Application.UseWaitCursor = False
            Application.DoEvents()

            MsgBox("Research Agents reset", vbInformation, Application.ProductName)
        End If

    End Sub

    Private Sub mnuResetIndustryJobs_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetIndustryJobs.Click
        Dim Response As MsgBoxResult
        Dim SQL As String

        Response = MsgBox("This will reset all stored Industry Jobs for this character." & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            SQL = "DELETE FROM INDUSTRY_JOBS WHERE InstallerID =" & SelectedCharacter.ID & " AND JobType =" & ScanType.Personal
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "UPDATE ESI_CHARACTER_DATA SET INDUSTRY_JOBS_CACHE_DATE = NULL WHERE CHARACTER_ID =" & CStr(SelectedCharacter.ID)
            EVEDB.ExecuteNonQuerySQL(SQL)

            Application.UseWaitCursor = False
            Application.DoEvents()

            MsgBox("Industry Jobs reset", vbInformation, Application.ProductName)
        End If

    End Sub

    Private Sub mnuResetIgnoredBPs_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetIgnoredBPs.Click
        Dim Response As MsgBoxResult
        Dim SQL As String

        Response = MsgBox("This will reset all blueprints to non-ignored" & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            SQL = "UPDATE ALL_BLUEPRINTS SET IGNORE = 0"
            EVEDB.ExecuteNonQuerySQL(SQL)

            Application.UseWaitCursor = False
            Application.DoEvents()

            MsgBox("Ignored Blueprints reset", vbInformation, Application.ProductName)
        End If
    End Sub

    Private Sub mnuResetAssets_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetAssets.Click
        Dim Response As MsgBoxResult
        Dim SQL As String

        Response = MsgBox("This will reset all stored Assets for this character." & Environment.NewLine & "Are you sure you want to do this?", vbYesNo, Application.ProductName)

        If Response = vbYes Then
            Application.UseWaitCursor = True
            Application.DoEvents()

            ' Personal
            SQL = "DELETE FROM ASSETS WHERE ID =" & SelectedCharacter.ID
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "UPDATE ESI_CHARACTER_DATA SET ASSETS_CACHE_DATE = NULL WHERE CHARACTER_ID =" & CStr(SelectedCharacter.ID)
            EVEDB.ExecuteNonQuerySQL(SQL)

            ' Corp
            SQL = "DELETE FROM ASSETS WHERE ID =" & SelectedCharacter.CharacterCorporation.CorporationID
            EVEDB.ExecuteNonQuerySQL(SQL)

            SQL = "UPDATE ESI_CORPORATION_DATA SET ASSETS_CACHE_DATE = NULL WHERE CORPORATION_ID =" & CStr(SelectedCharacter.CharacterCorporation.CorporationID)
            EVEDB.ExecuteNonQuerySQL(SQL)

            ' Reload the asset variables for the character, which will load nothing but clear the assets out
            Call SelectedCharacter.GetAssets().LoadAssets(SelectedCharacter.ID, SelectedCharacter.CharacterTokenData, UserApplicationSettings.LoadAssetsonStartup)
            Call SelectedCharacter.CharacterCorporation.GetAssets().LoadAssets(SelectedCharacter.CharacterCorporation.CorporationID, SelectedCharacter.CharacterTokenData, UserApplicationSettings.LoadAssetsonStartup)

            Application.UseWaitCursor = False
            Application.DoEvents()

            MsgBox("Assets reset", vbInformation, Application.ProductName)
        End If

    End Sub

    Private Sub mnuCurrentIndustryJobs_Click(sender As System.Object, e As System.EventArgs) Handles mnuCurrentIndustryJobs.Click
        Dim f1 As New frmIndustryJobsViewer
        f1.Show()
    End Sub

    Private Sub mnuResetESIMarketPrices_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetESIMarketPrices.Click
        Call ResetESIAdjustedMarketPrices()
    End Sub

    Private Sub mnuResetESIIndustryFacilities_Click(sender As System.Object, e As System.EventArgs) Handles mnuResetESIIndustryFacilities.Click
        Call ResetESIIndustryFacilities()
    End Sub

    ' Checks the ME and TE boxes to make sure they are ok and errors if not
    Private Function CorrectMETE(ByVal inME As String, ByVal inTE As String, ByRef METextBox As TextBox, ByRef TETextBox As TextBox) As Boolean

        If Not IsNumeric(inME) Or Trim(inME) = "" Then
            MsgBox("Invalid ME Value", vbExclamation)
            METextBox.SelectAll()
            METextBox.Focus()
            Return False
        End If

        If Not IsNumeric(inTE) Or Trim(inTE) = "" Then
            MsgBox("Invalid TE Value", vbExclamation)
            TETextBox.SelectAll()
            TETextBox.Focus()
            Return False
        End If

        Return True

    End Function

    Private Sub tabMain_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tabMain.Click
        If tabMain.SelectedTab.Name = "tabDatacores" Then
            If FirstShowDatacores Then
                ' Load up the data first
                Me.Cursor = Cursors.WaitCursor

                ' DC Skills and Levels plus the standings
                Call LoadDatacoreTab()

                Me.Cursor = Cursors.Default
                FirstShowDatacores = False ' Don't run this for successive clicks to this tab
            End If
        ElseIf tabMain.SelectedTab.Name = "tabMining" Then
            If FirstShowMining Then
                ' Load up the data first
                Me.Cursor = Cursors.WaitCursor

                ' DC Skills and Levels plus the standings
                Call LoadMiningTab()

                Me.Cursor = Cursors.Default

                FirstShowMining = False ' Don't run for successive clicks
            End If
        End If
    End Sub

    Private Sub mnuInventionResultsTracking_Click(sender As System.Object, e As System.EventArgs)
        Dim f1 As New frmInventionMonitor
        f1.ShowDialog()
    End Sub

    Private Sub mnuCurrentResearchAgents_Click(sender As System.Object, e As System.EventArgs) Handles mnuCurrentResearchAgents.Click
        Dim f1 As New frmResearchAgents
        f1.Show()
    End Sub

    Private Sub mnuSelectionAddChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSelectionAddChar.Click

        ' Open up the default select box here
        Dim f2 = New frmSetCharacterDefault
        f2.ShowDialog()

        ' Only allow selecting a default if they are registered
        If AppRegistered() Then
            mnuSelectDefaultChar.Enabled = True
        Else
            mnuSelectDefaultChar.Enabled = False
        End If

        Call LoadCharacterNamesinMenu()

        ' Reinit form
        Call ResetTabs()

    End Sub

    Private Sub mnuSelectionManageCharacters_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSelectionManageCharacters.Click
        Dim f1 As New frmManageAccounts

        Call f1.ShowDialog()

        ' Only allow selecting a default if they registered the program
        If AppRegistered() Then
            mnuSelectionAddChar.Enabled = True
            mnuSelectDefaultChar.Enabled = True
        Else
            mnuSelectionAddChar.Enabled = False
            mnuSelectDefaultChar.Enabled = False
            ' Reload the list
            Call LoadCharacterNamesinMenu()
        End If

        ' Default character set, now set the panel if it changed
        If SelectedCharacter.Name <> mnuCharacter.Text.Substring(mnuCharacter.Text.IndexOf(":") + 2) Then
            ' If we returned, we got a default character set
            Call ResetTabs()
            Call LoadCharacterNamesinMenu()
        End If

    End Sub

    Private Sub mnuRegisterProgram_Click(sender As Object, e As EventArgs) Handles mnuRegisterProgram.Click
        Dim f1 As New frmLoadESIAuthorization
        f1.ShowDialog()
        f1.Close()

        Dim ApplicationSettings As AppRegistrationInformationSettings = AllSettings.LoadAppRegistrationInformationSettings

        ' If they registered the program, let them add characters now
        If AppRegistered() Then
            mnuSelectionAddChar.Enabled = True
            Dim f2 As New frmSetCharacterDefault
            f2.ShowDialog()
            Call LoadCharacterNamesinMenu()
        End If

    End Sub

    Private Sub mnuItemUpdatePrices_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuItemUpdatePrices.Click
        Dim f1 = New frmManualPriceUpdate
        f1.ShowDialog()
        Call ResetRefresh()
    End Sub

    Private Sub mnuCheckforUpdates_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCheckforUpdates.Click
        Me.Cursor = Cursors.WaitCursor
        Application.DoEvents()
        Call CheckForUpdates(True, Me.Icon)
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub btnCancelUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancelUpdate.Click
        CancelUpdatePrices = True
    End Sub

    Private Sub mnuSelectionExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuSelectionExit.Click
        End
    End Sub

    Private Sub mnuSelectionShoppingList_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSelectionShoppingList.Click
        Call ShowShoppingList()
    End Sub

    Private Sub mnuViewAssets_Click(sender As Object, e As EventArgs) Handles mnuViewAssets.Click
        ' Make sure it's not disposed
        If IsNothing(frmDefaultAssets) Then
            ' Make new form
            frmDefaultAssets = New frmAssetsViewer(AssetWindow.DefaultView)
        Else
            If frmDefaultAssets.IsDisposed Then
                ' Make new form
                frmDefaultAssets = New frmAssetsViewer(AssetWindow.DefaultView)
            End If
        End If

        ' Now open the Asset List
        frmDefaultAssets.Show()
        frmDefaultAssets.Focus()

        Application.DoEvents()
    End Sub

    Private Sub mnuManageBlueprintsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuManageBlueprintsToolStripMenuItem.Click
        Dim f1 = New frmBlueprintManagement
        f1.Show()
        Call ResetRefresh()
        ' Reload the bp if there is one loaded so we get the most updated bps
        'If Not IsNothing(SelectedBlueprint) Then
        '    Call SelectBlueprint(False)
        'End If
    End Sub

    Private Sub mnuSelectDefaultChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSelectDefaultChar.Click
        Dim f1 = New frmSetCharacterDefault
        Dim PreviousChar As String

        PreviousChar = SelectedCharacter.Name
        f1.ShowDialog()
        ' If we returned, we got a default character set
        Call LoadCharacterNamesinMenu()

        ' If they cancel or choose the same one, don't re load everything
        If PreviousChar <> SelectedCharacter.Name Then
            Call ResetTabs()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub pnlShoppingList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pnlShoppingList.Click
        Call ShowShoppingList()
    End Sub

    Private Sub mnuSelectionShoppingList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ShowShoppingList()
    End Sub

    Private Sub mnuSelectionAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSelectionAbout.Click
        Dim f1 = New frmAbout
        ' Open the Shopping List
        f1.ShowDialog()
    End Sub

    Private Sub mnuCharacterSkills_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCharacterSkills.Click
        Call OpenCharacterSkills()
    End Sub

    Private Sub pnlSkills_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pnlSkills.Click
        Call OpenCharacterSkills()
    End Sub

    Private Sub OpenCharacterSkills()
        Dim f1 = New frmCharacterSkills
        ' Open the character screen
        SkillsUpdated = False
        f1.ShowDialog()

        If SkillsUpdated Then
            Call UpdateSkillPanel()
            ' Need to reload screens that have skills displayed on it
            Call InitDatacoreTab()
            Call InitMiningTab()

            ' Refresh the BP Tab if there is a blueprint selected since skills could affect build
            If Not IsNothing(SelectedBlueprint) Then
                Call RefreshBP()
            End If
        End If
    End Sub

    Private Sub mnuCharacterStandings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCharacterStandings.Click
        Dim f1 = New frmCharacterStandings
        ' Open the character screen
        f1.ShowDialog()
    End Sub

    Private Sub mnuUserSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuUserSettings.Click
        Dim f1 = New frmSettings
        ' Open the settings form
        f1.ShowDialog()
    End Sub

    Private Sub ShowShoppingList()

        ' Make sure it's not disposed
        If frmShop.IsDisposed Then
            ' Make new form
            frmShop = New frmShoppingList
        End If

        ' First refresh the lists
        frmShop.RefreshLists()

        ' Now open the Shopping List
        frmShop.Show()
        frmShop.Focus()

        Application.DoEvents()

    End Sub

    Private Sub UpdateSkillPanel()
        If UserApplicationSettings.AllowSkillOverride Then
            pnlSkills.ForeColor = Color.Red
            pnlSkills.Text = "Skills Overridden"
        Else
            pnlSkills.ForeColor = Color.Black
            pnlSkills.Text = "Skills Loaded"
        End If
    End Sub

    Public Sub ResetTabs(Optional ResetBPTab As Boolean = True)
        ' Init all forms
        Me.Cursor = Cursors.WaitCursor
        Call InitBPTab(ResetBPTab)
        Call InitDatacoreTab()
        Call InitManufacturingTab()
        Call InitReactionsTab()
        Call InitUpdatePricesTab()
        Call InitMiningTab()

        ' Update skill override
        Call UpdateSkillPanel()

        ' New Char so load the max lines
        If Not IsNothing(SelectedCharacter.Skills) Then ' 3387 mass production, 24625 adv mass production, 3406 laboratory efficiency, 24524 adv laboratory operation
            MaximumProductionLines = SelectedCharacter.Skills.GetSkillLevel(3387) + SelectedCharacter.Skills.GetSkillLevel(24525) + 1
            MaximumLaboratoryLines = SelectedCharacter.Skills.GetSkillLevel(3406) + SelectedCharacter.Skills.GetSkillLevel(24524) + 1
        Else
            MaximumProductionLines = 1
            MaximumLaboratoryLines = 1
        End If

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub mnuRestoreDefaultBP_Click(sender As System.Object, e As System.EventArgs) Handles mnuRestoreDefaultBP.Click
        Call AllSettings.SetDefaultBPSettings()

        ' Also need to reset the shared variables
        UserApplicationSettings.CheckBuildBuy = DefaultSettings.DefaultCheckBuildBuy

        ' Save them
        Call AllSettings.SaveBPSettings(AllSettings.GetBPSettings)
        Call AllSettings.SaveApplicationSettings(UserApplicationSettings)

        ' Load them again
        UserBPTabSettings = AllSettings.LoadBPSettings()
        UserApplicationSettings = AllSettings.LoadApplicationSettings()

        ' Reload the tab
        Call InitBPTab()

        MsgBox("BP Tab Default Settings Restored", vbInformation, Application.ProductName)

    End Sub

    Private Sub mnuRestoreDefaultUpdatePrices_Click(sender As System.Object, e As System.EventArgs) Handles mnuRestoreDefaultUpdatePrices.Click
        Call AllSettings.SetDefaultUpdatePriceSettings()
        ' Save them
        Call AllSettings.SaveUpdatePricesSettings(AllSettings.GetUpdatePricesSettings)
        ' Load them again
        UserUpdatePricesTabSettings = AllSettings.LoadUpdatePricesSettings()

        ' Reload the tab
        Call InitUpdatePricesTab()

        MsgBox("Update Prices Tab Default Settings Restored", vbInformation, Application.ProductName)

    End Sub

    Private Sub mnuRestoreDefaultReactions_Click(sender As System.Object, e As System.EventArgs) Handles mnuRestoreDefaultReactions.Click
        Call AllSettings.SetDefaultReactionSettings()
        ' Save them
        Call AllSettings.SaveReactionSettings(AllSettings.GetReactionSettings)
        ' Load them again
        UserReactionTabSettings = AllSettings.LoadReactionSettings()

        ' Reload the tab
        Call InitReactionsTab()

        MsgBox("Reactions Tab Default Settings Restored", vbInformation, Application.ProductName)

    End Sub

    Private Sub mnuRestoreDefaultDatacores_Click(sender As System.Object, e As System.EventArgs) Handles mnuRestoreDefaultDatacores.Click
        Call AllSettings.SetDefaultDatacoreSettings()
        ' Save them
        Call AllSettings.SaveDatacoreSettings(AllSettings.GetDatacoreSettings)
        ' Load them again
        UserDCTabSettings = AllSettings.LoadDatacoreSettings()

        ' Reload the tab
        Call InitDatacoreTab()

        MsgBox("Datacores Tab Default Settings Restored", vbInformation, Application.ProductName)

    End Sub

    Private Sub mnuRestoreDefaultManufacturing_Click(sender As System.Object, e As System.EventArgs) Handles mnuRestoreDefaultManufacturing.Click
        Call AllSettings.SetDefaultManufacturingSettings()

        ' Also need to reset the shared variables
        UserApplicationSettings.DefaultBPME = DefaultSettings.DefaultSettingME
        UserApplicationSettings.DefaultBPTE = DefaultSettings.DefaultSettingTE

        ' Save them
        Call AllSettings.SaveManufacturingSettings(AllSettings.GetManufacturingSettings)
        Call AllSettings.SaveApplicationSettings(UserApplicationSettings)

        ' Load them again
        UserManufacturingTabSettings = AllSettings.LoadManufacturingSettings()
        UserApplicationSettings = AllSettings.LoadApplicationSettings()

        ' Reload the tab
        Call InitManufacturingTab()
        ' Also update these shared form variables
        'cmbBPBuildMod.Text = DefaultSettings.DefaultBuildSlotModifier

        MsgBox("Manufacturing Tab Default Settings Restored", vbInformation, Application.ProductName)

    End Sub

    Private Sub mnuRestoreDefaultMining_Click(sender As System.Object, e As System.EventArgs) Handles mnuRestoreDefaultMining.Click
        Call AllSettings.SetDefaultMiningSettings()
        ' Save them
        Call AllSettings.SaveMiningSettings(AllSettings.GetMiningSettings)
        ' Load them again
        UserMiningTabSettings = AllSettings.LoadMiningSettings()

        ' Reload the tab
        Call InitMiningTab()

        MsgBox("Minings Tab Default Settings Restored", vbInformation, Application.ProductName)
    End Sub

    Private Sub chkBPIncludeCopyTime_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIncludeCopyTime.CheckedChanged
        If Not FirstLoad And Not UpdatingInventionChecks Then
            ' Set the copy time check
            BPTabFacility.GetFacility(ProductionType.Copying).IncludeActivityTime = chkBPIncludeCopyTime.Checked
            If Not IsNothing(SelectedBlueprint) Then
                ' Use the original ME and TE values when they change the meta level
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)

            End If
        End If
    End Sub

    Private Sub chkBPIncludeCopyCosts_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIncludeCopyCosts.CheckedChanged
        If Not FirstLoad And Not UpdatingInventionChecks Then
            ' Include copy costs
            BPTabFacility.GetFacility(ProductionType.Copying).IncludeActivityCost = chkBPIncludeCopyCosts.Checked
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
            End If
        End If
    End Sub

    Private Sub chkBPIncludeInventionTime_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIncludeInventionTime.CheckedChanged
        If Not FirstLoad And Not UpdatingInventionChecks Then
            ' Include invention time
            BPTabFacility.GetFacility(ProductionType.Invention).IncludeActivityTime = chkBPIncludeInventionTime.Checked
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
            End If
        End If
    End Sub

    Private Sub chkBPIncludeInventionCosts_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIncludeInventionCosts.CheckedChanged
        If Not FirstLoad And Not UpdatingInventionChecks Then
            ' Include cost for invention
            BPTabFacility.GetFacility(ProductionType.Invention).IncludeActivityCost = chkBPIncludeInventionCosts.Checked
            ' Use the original ME and TE values when they change the meta level
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
            End If
        End If
    End Sub

    Private Sub chkBPIncludeT3Time_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIncludeT3Time.CheckedChanged
        If Not FirstLoad And Not UpdatingInventionChecks Then
            ' Set the time for T3 invention
            BPTabFacility.GetFacility(ProductionType.T3Invention).IncludeActivityTime = chkBPIncludeT3Time.Checked
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
            End If
        End If
    End Sub

    Private Sub chkBPIncludeT3Costs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIncludeT3Costs.CheckedChanged
        If Not FirstLoad And Not UpdatingInventionChecks Then
            ' Set the usage for T3 invention
            BPTabFacility.GetFacility(ProductionType.T3Invention).IncludeActivityCost = chkBPIncludeT3Costs.Checked
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
            End If
        End If
    End Sub

    Private Sub UpdateIndustryFacilitiesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles mnuUpdateIndustryFacilities.Click
        Call UpdateESIIndustryFacilities()
    End Sub

    Private Sub UpdateMarketPricesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles mnuUpdateESIMarketPrices.Click
        Call UpdateESIMarketPrices()
    End Sub

    ' Function runs the ESI update for system indicies
    Private Sub UpdateESIIndustryFacilities()
        Dim ESIData As New ESI
        Dim f1 As New frmStatus

        Application.UseWaitCursor = True
        Call f1.Show()
        Application.DoEvents()

        ' Always do indicies first since facilities has a field it uses
        If ESIData.UpdateIndustryFacilties(f1.lblStatus, f1.pgStatus) Then
            ' Reload the industry facilities now
            Call BPTabFacility.InitializeFacilities(FacilityView.FullControls)

            ' Refresh the BP Tab if there is a blueprint selected
            If Not IsNothing(SelectedBlueprint) Then
                Call RefreshBP(True)
            End If

            MsgBox("Industry System Indicies and Facilities Updated", vbInformation, Application.ProductName)
        End If

        f1.Dispose()
        Application.UseWaitCursor = False

    End Sub

    ' Function runs the ESI update for market prices
    Private Sub UpdateESIMarketPrices()
        Dim ESIData As New ESI
        Dim f1 As New frmStatus

        Application.UseWaitCursor = True
        Call f1.Show()
        Application.DoEvents()
        If ESIData.UpdateAdjAvgMarketPrices(f1.lblStatus, f1.pgStatus) Then

            ' Update all the prices in the program
            Call UpdateProgramPrices()

            MsgBox("Market Prices Updated", vbInformation, Application.ProductName)
        End If

        f1.Dispose()
        Application.UseWaitCursor = False

    End Sub

#End Region

#Region "InlineListUpdate"

    ' Determines where to show the text box when clicking on the list sent
    Private Sub ListClicked(ListRef As ListView, sender As Object, e As System.Windows.Forms.MouseEventArgs)
        Dim iSubIndex As Integer = 0

        ' Hide the text box when a new line is selected
        txtListEdit.Hide()
        cmbEdit.Hide()

        CurrentRow = ListRef.GetItemAt(e.X, e.Y) ' which listviewitem was clicked
        SelectedGrid = ListRef

        If CurrentRow Is Nothing Then
            Exit Sub
        End If

        CurrentCell = CurrentRow.GetSubItemAt(e.X, e.Y)  ' which subitem was clicked

        ' Determine where the previous and next item boxes will be based on what they clicked - used in tab event handling
        Call SetNextandPreviousCells(ListRef)

        ' See which column has been clicked
        iSubIndex = CurrentRow.SubItems.IndexOf(CurrentCell)

        If ListRef.Name <> lstPricesView.Name And ListRef.Name <> lstMineGrid.Name _
            And ListRef.Name <> lstRawPriceProfile.Name And ListRef.Name <> lstManufacturedPriceProfile.Name Then
            ' Set the columns that can be edited, just ME and Price
            If iSubIndex = 2 Or iSubIndex = 3 Then

                If iSubIndex = 2 Then
                    MEUpdate = True
                Else
                    MEUpdate = False
                End If

                If iSubIndex = 3 Then
                    PriceUpdate = True
                Else
                    PriceUpdate = False
                End If

                ' For the update grids in the Blueprint Tab, only show the box if
                ' 1 - If the ME is clicked and it has something other than a '-' in it (meaning no BP)
                ' 2 - If the Price is clicked and the ME box has '-' in it
                If (CurrentRow.SubItems(2).Text <> "-" And MEUpdate) Or (CurrentRow.SubItems(2).Text = "-" And PriceUpdate) Then
                    Call ShowEditBox(ListRef)
                End If

            End If

        ElseIf ListRef.Name = lstPricesView.Name Or ListRef.Name = lstMineGrid.Name Then ' Price update for update prices and mining grid

            ' Only process the price box logic on rows that are unrefined and compressed ore on mining tab
            If ListRef.Name = lstMineGrid.Name And CurrentRow.SubItems(2).Text = "Refined" Then
                Exit Sub
            End If

            ' Set the columns that can be edited, just Price
            If iSubIndex = 3 Then
                Call ShowEditBox(ListRef)
                PriceUpdate = True
            End If

        ElseIf ListRef.Name = lstRawPriceProfile.Name Or ListRef.Name = lstManufacturedPriceProfile.Name Then

            If iSubIndex > 0 Then
                ' Reset update type
                Call SetPriceProfileVariables(iSubIndex)
                Call ShowEditBox(ListRef)
            End If

        End If

    End Sub

    ' For updating the items in the list by clicking on them
    Private Sub ProcessKeyDownEdit(SentKey As Keys, ListRef As ListView)
        Dim SQL As String = ""
        Dim rsData As SQLiteDataReader

        Dim MEValue As String = ""
        Dim PriceValue As Double = 0
        Dim PriceUpdated As Boolean = False

        ' Change blank entry to 0
        If Trim(txtListEdit.Text) = "" Then
            txtListEdit.Text = "0"
        End If

        DataUpdated = False

        ' If they hit enter or tab away, mark the BP as owned in the DB with the values entered
        If (SentKey = Keys.Enter Or SentKey = Keys.ShiftKey Or SentKey = Keys.Tab) And DataEntered Then

            ' Check the input first
            If Not IsNumeric(txtListEdit.Text) And MEUpdate Then
                MsgBox("Invalid ME Value", vbExclamation)
                Exit Sub
            End If

            If Not IsNumeric(txtListEdit.Text) And PriceUpdate Then
                MsgBox("Invalid Price Value", vbExclamation)
                Exit Sub
            End If

            ' Save the data depending on what we are updating
            If MEUpdate Then
                MEValue = txtListEdit.Text
            End If

            If PriceUpdate Then
                PriceValue = CDbl(txtListEdit.Text)
            End If

            ' Now do the update for the grids
            If ListRef.Name <> lstPricesView.Name And ListRef.Name <> lstMineGrid.Name And
                ListRef.Name <> lstRawPriceProfile.Name And ListRef.Name <> lstManufacturedPriceProfile.Name Then
                ' BP Grid update

                ' Check the numbers, if the same then don't update
                If MEValue = CurrentRow.SubItems(2).Text And PriceValue = CDbl(CurrentRow.SubItems(3).Text) Then
                    ' Skip down
                    GoTo Tabs
                End If

                ' First, see if we are updating an ME or a price, then deal with each separately
                If MEUpdate Then
                    ' First we need to look up the Blueprint ID
                    SQL = "SELECT ALL_BLUEPRINTS.BLUEPRINT_ID, ALL_BLUEPRINTS.BLUEPRINT_NAME, TECH_LEVEL, "
                    SQL = SQL & "CASE WHEN FAVORITE IS NULL THEN 0 ELSE FAVORITE END AS FAVORITE, IGNORE, "
                    SQL = SQL & "CASE WHEN TE Is NULL THEN 0 ELSE TE END AS BP_TE "
                    SQL = SQL & "FROM ALL_BLUEPRINTS LEFT JOIN OWNED_BLUEPRINTS ON ALL_BLUEPRINTS.BLUEPRINT_ID = OWNED_BLUEPRINTS.BLUEPRINT_ID  "
                    SQL = SQL & "WHERE ITEM_NAME = '" & CurrentRow.SubItems(0).Text & "'"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    rsData = DBCommand.ExecuteReader
                    rsData.Read()

                    ' If they update the ME of the blueprint, then we mark it as Owned and a 0 for TE value, but set the type depending on the bp loaded
                    Dim TempBPType As BPType
                    Dim AdditionalCost As Double
                    Dim TempTE As Integer = rsData.GetInt32(5)

                    If rsData.GetInt64(2) = BPTechLevel.T1 Then
                        ' T1 BPO
                        TempBPType = BPType.Original
                    Else
                        ' Remaining T2 and T3 must be invited
                        TempBPType = BPType.InventedBPC
                    End If

                    ' Check additional costs for saving with this bp
                    If IsNumeric(txtBPAddlCosts.Text) Then
                        AdditionalCost = CDbl(txtBPAddlCosts.Text)
                    Else
                        AdditionalCost = 0
                    End If

                    ' If there is no TE for an invented BPC then set it to the base
                    If TempBPType = BPType.InventedBPC And TempTE = 0 Then
                        TempTE = BaseT2T3TE
                    End If

                    Call UpdateBPinDB(rsData.GetInt64(0), rsData.GetString(1), CInt(MEValue), TempTE, TempBPType, CInt(MEValue), 0,
                                      CBool(rsData.GetInt32(3)), CBool(rsData.GetInt32(4)), AdditionalCost)

                    ' Mark the line with white color since it's no longer going to be unowned
                    CurrentRow.BackColor = Color.White

                    rsData.Close()

                Else ' Price per unit update

                    SQL = "UPDATE ITEM_PRICES SET PRICE = " & CStr(CDbl(txtListEdit.Text)) & ", PRICE_TYPE = 'User' WHERE ITEM_NAME = '" & CurrentRow.SubItems(0).Text & "'"
                    Call EVEDB.ExecuteNonQuerySQL(SQL)

                    ' Mark the line text with black incase it is red for no price
                    CurrentRow.ForeColor = Color.Black

                    PriceUpdated = True

                End If

                ' Update the data in the current row
                CurrentRow.SubItems(2).Text = CStr(MEValue)
                CurrentRow.SubItems(3).Text = FormatNumber(PriceValue, 2)

                ' For both ME and Prices, we need to re-calculate the blueprint (hit the Refresh Button) to reflect the new numbers
                ' First save the current grid for locations
                RefreshingGrid = True
                Call RefreshBP()
                RefreshingGrid = False

            ElseIf ListRef.Name <> lstRawPriceProfile.Name And ListRef.Name <> lstManufacturedPriceProfile.Name Then
                ' Price List Update
                SQL = "UPDATE ITEM_PRICES SET PRICE = " & CStr(CDbl(txtListEdit.Text)) & ", PRICE_TYPE = 'User' WHERE ITEM_ID = " & CurrentRow.SubItems(0).Text
                Call EVEDB.ExecuteNonQuerySQL(SQL)

                ' Change the value in the price grid, but don't update the grid
                CurrentRow.SubItems(3).Text = FormatNumber(txtListEdit.Text, 2)

                PriceUpdated = True
            Else
                ' Price Profile update
                Dim RawMat As String
                If ListRef.Name = lstRawPriceProfile.Name Then
                    RawMat = "1"
                Else
                    RawMat = "0"
                End If

                ' See if they have the profile set already
                SQL = "SELECT 'X' FROM PRICE_PROFILES WHERE ID = " & CStr(SelectedCharacter.ID) & " "
                SQL = SQL & "AND GROUP_NAME = '" & CurrentRow.SubItems(0).Text & "' "
                SQL = SQL & "AND RAW_MATERIAL = " & RawMat

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                rsData = DBCommand.ExecuteReader

                If rsData.Read() Then
                    ' Update
                    SQL = "UPDATE PRICE_PROFILES SET "
                    If PriceTypeUpdate Then
                        ' Save current region/system
                        SQL = SQL & "PRICE_TYPE = '" & cmbEdit.Text & "' "
                        CurrentRow.SubItems(1).Text = cmbEdit.Text
                    ElseIf PriceSystemUpdate Then
                        ' Just update system, save others
                        SQL = SQL & "SOLAR_SYSTEM_NAME = '" & cmbEdit.Text & "' "
                        CurrentRow.SubItems(3).Text = cmbEdit.Text
                    ElseIf PriceRegionUpdate Then
                        ' Set region, but set system to all systems (blank)
                        SQL = SQL & "REGION_NAME ='" & cmbEdit.Text & "', SOLAR_SYSTEM_NAME = 'All Systems' "
                        CurrentRow.SubItems(2).Text = cmbEdit.Text
                        CurrentRow.SubItems(3).Text = AllSystems
                    ElseIf PriceModifierUpdate Then
                        Dim PM As Double = CDbl(txtListEdit.Text.Replace("%", "")) / 100
                        SQL = SQL & "PRICE_MODIFIER = " & CStr(PM) & " "
                        CurrentRow.SubItems(4).Text = FormatPercent(PM, 1)
                    End If

                    SQL = SQL & "WHERE ID = " & CStr(SelectedCharacter.ID) & " "
                    SQL = SQL & "AND GROUP_NAME ='" & CurrentRow.SubItems(0).Text & "' "
                    SQL = SQL & "AND RAW_MATERIAL = " & RawMat

                Else
                    ' Insert new record
                    Dim TempPercent As String = CStr(CDbl(CurrentRow.SubItems(4).Text.Replace("%", "")) / 100)
                    SQL = "INSERT INTO PRICE_PROFILES VALUES (" & CStr(SelectedCharacter.ID) & ",'" & CurrentRow.SubItems(0).Text & "','"
                    If PriceTypeUpdate Then
                        ' Save current region/system
                        SQL = SQL & FormatDBString(cmbEdit.Text) & "','" & CurrentRow.SubItems(2).Text & "','" & CurrentRow.SubItems(3).Text & "'," & TempPercent & "," & RawMat & ")"
                        CurrentRow.SubItems(1).Text = cmbEdit.Text
                    ElseIf PriceSystemUpdate Then
                        ' Just update system, save others
                        SQL = SQL & CurrentRow.SubItems(1).Text & "','" & CurrentRow.SubItems(2).Text & "','" & FormatDBString(cmbEdit.Text) & "'," & TempPercent & "," & RawMat & ")"
                        CurrentRow.SubItems(3).Text = cmbEdit.Text
                    ElseIf PriceRegionUpdate Then
                        ' Set region, but set system to all systems (blank)
                        SQL = SQL & CurrentRow.SubItems(1).Text & "','" & FormatDBString(cmbEdit.Text) & "','All Systems'," & TempPercent & "," & RawMat & ")"
                        ' Set the text
                        CurrentRow.SubItems(2).Text = cmbEdit.Text
                        CurrentRow.SubItems(3).Text = AllSystems
                    ElseIf PriceModifierUpdate Then
                        ' Save current region/system/type
                        SQL = SQL & CurrentRow.SubItems(1).Text & "','" & CurrentRow.SubItems(2).Text & "','" & CurrentRow.SubItems(3).Text & "',"
                        Dim PM As Double = CDbl(txtListEdit.Text.Replace("%", "")) / 100
                        SQL = SQL & CStr(PM) & "," & RawMat & ")"
                        CurrentRow.SubItems(4).Text = FormatPercent(PM, 1)
                    End If

                End If

                Call EVEDB.ExecuteNonQuerySQL(SQL)

                ' Reset these
                PriceTypeUpdate = False
                PriceRegionUpdate = False
                PriceSystemUpdate = False
                PreviousPriceType = ""
                PreviousRegion = ""
                PreviousSystem = ""
                PriceUpdated = False

            End If

            ' If we updated a price, then update the program everywhere to be consistent
            If PriceUpdated Then
                IgnoreFocus = True
                Call UpdateProgramPrices()
                IgnoreFocus = False
            End If

            ' Play sound to indicate update complete
            If PriceUpdated Then
                Call PlayNotifySound()
            End If

            ' Reset text they entered if tabbed
            If SentKey = Keys.ShiftKey Or SentKey = Keys.Tab Then
                txtListEdit.Text = ""
                cmbEdit.Text = ""
            End If

            If SentKey = Keys.Enter Then
                ' Just refresh and select the current row
                CurrentRow.Selected = True
                txtListEdit.Visible = False
            End If

            ' Data updated, so reset
            DataEntered = False
            DataUpdated = True

        End If

Tabs:
        ' If they hit tab, then tab to the next cell
        If SentKey = Keys.Tab Then
            If CurrentRow.Index = -1 Then
                ' Reset the current row based on the original click
                CurrentRow = ListRef.GetItemAt(SavedListClickLoc.X, SavedListClickLoc.Y)
                CurrentCell = CurrentRow.GetSubItemAt(SavedListClickLoc.X, SavedListClickLoc.Y)
                ' Reset the next and previous cells
                SetNextandPreviousCells(ListRef)
            End If

            CurrentCell = NextCell
            ' Reset these each time
            Call SetNextandPreviousCells(ListRef, "Next")
            If CurrentRow.Index = 0 Then
                ' Scroll to top
                ListRef.Items.Item(0).Selected = True
                ListRef.EnsureVisible(0)
                ListRef.Update()
            Else
                ' Make sure the row is visible
                ListRef.EnsureVisible(CurrentRow.Index)
            End If

            ' Show the text box
            Call ShowEditBox(ListRef)
        End If

        ' If shift+tab, then go to the previous cell 
        If SentKey = Keys.ShiftKey Then
            If CurrentRow.Index = -1 Then
                ' Reset the current row based on the original click
                CurrentRow = ListRef.GetItemAt(SavedListClickLoc.X, SavedListClickLoc.Y)
                CurrentCell = CurrentRow.GetSubItemAt(SavedListClickLoc.X, SavedListClickLoc.Y)
                ' Reset the next and previous cells
                SetNextandPreviousCells(ListRef)
            End If

            CurrentCell = PreviousCell
            ' Reset these each time
            Call SetNextandPreviousCells(ListRef, "Previous")
            If CurrentRow.Index = ListRef.Items.Count - 1 Then
                ' Scroll to bottom
                ListRef.Items.Item(ListRef.Items.Count - 1).Selected = True
                ListRef.EnsureVisible(ListRef.Items.Count - 1)
                ListRef.Update()
            Else
                ' Make sure the row is visible
                ListRef.EnsureVisible(CurrentRow.Index)
            End If

            ' Show the text box
            Call ShowEditBox(ListRef)
        End If

    End Sub

    ' Determines where the previous and next item boxes will be based on what they clicked - used in tab event handling
    Private Sub SetNextandPreviousCells(ListRef As ListView, Optional CellType As String = "")
        Dim iSubIndex As Integer = 0

        ' Normal Row
        If CellType = "Next" Then
            CurrentRow = NextCellRow
        ElseIf CellType = "Previous" Then
            CurrentRow = PreviousCellRow
        End If

        ' Get index of column
        iSubIndex = CurrentRow.SubItems.IndexOf(CurrentCell)

        ' Get next and previous rows. If at end, wrap to top. If at top, wrap to bottom
        If ListRef.Items.Count = 1 Then
            NextRow = CurrentRow
            PreviousRow = CurrentRow
        ElseIf CurrentRow.Index <> ListRef.Items.Count - 1 And CurrentRow.Index <> 0 Then
            ' Not the last line, so set the next and previous
            NextRow = ListRef.Items.Item(CurrentRow.Index + 1)
            PreviousRow = ListRef.Items.Item(CurrentRow.Index - 1)
        ElseIf CurrentRow.Index = 0 Then
            NextRow = ListRef.Items.Item(CurrentRow.Index + 1)
            ' Wrap to bottom
            PreviousRow = ListRef.Items.Item(ListRef.Items.Count - 1)
        ElseIf CurrentRow.Index = ListRef.Items.Count - 1 Then
            ' Need to wrap up to top
            NextRow = ListRef.Items.Item(0)
            PreviousRow = ListRef.Items.Item(CurrentRow.Index - 1)
        End If

        If ListRef.Name <> lstPricesView.Name And ListRef.Name <> lstMineGrid.Name _
            And ListRef.Name <> lstRawPriceProfile.Name And ListRef.Name <> lstManufacturedPriceProfile.Name Then

            ' For the update grids in the Blueprint Tab, only show the box if
            ' 1 - If the ME is clicked and it has something other than a '-' in it (meaning no BP)
            ' 2 - If the Price is clicked and the ME box has '-' in it

            ' The next row must be an ME or Price box on the next row 
            ' or a previous ME or price box on the previous row
            If iSubIndex = 2 Or iSubIndex = 3 Then
                ' Set the next and previous ME boxes (subitems)
                ' If the next row ME box is a '-' then the next row cell is Price
                If NextRow.SubItems(2).Text = "-" Then
                    NextCell = NextRow.SubItems.Item(3) ' Next row price box
                Else ' It can be the ME box in the next row
                    NextCell = NextRow.SubItems.Item(2) ' Next row ME box
                End If

                NextCellRow = NextRow

                'If the previous row ME box is a '-' then the previous row is Price
                If PreviousRow.SubItems(2).Text = "-" Then
                    PreviousCell = PreviousRow.SubItems.Item(3) ' Next row price box
                Else ' It can be the ME box in the next row
                    PreviousCell = PreviousRow.SubItems.Item(2) ' Next row ME box
                End If

                PreviousCellRow = PreviousRow

                If iSubIndex = 2 Then
                    MEUpdate = True
                    PriceUpdate = False
                Else
                    MEUpdate = False
                    PriceUpdate = True
                End If

            Else
                NextCell = Nothing
                PreviousCell = Nothing
                CurrentCell = Nothing
            End If

        ElseIf ListRef.Name = lstRawPriceProfile.Name Or ListRef.Name = lstManufacturedPriceProfile.Name Then

            If iSubIndex <> 0 Then
                ' Set the next and previous combo boxes
                If iSubIndex = 4 Then
                    NextCell = NextRow.SubItems.Item(1) ' Next now price type box
                    NextCellRow = NextRow
                Else
                    NextCell = CurrentRow.SubItems.Item(iSubIndex + 1) ' current row, next cell
                    NextCellRow = CurrentRow
                End If

                If iSubIndex = 1 Then
                    PreviousCell = PreviousRow.SubItems.Item(4) ' Previous row price mod
                    PreviousCellRow = PreviousRow
                Else
                    PreviousCell = CurrentRow.SubItems.Item(iSubIndex - 1) ' Same row, just back a cell
                    PreviousCellRow = CurrentRow
                End If

                ' Reset update type
                Call SetPriceProfileVariables(iSubIndex)

            Else
                NextCell = Nothing
                PreviousCell = Nothing
                CurrentCell = Nothing
            End If

        Else ' Price list 
            ' For this, just go up and down the rows
            NextCell = NextRow.SubItems.Item(3)
            NextCellRow = NextRow
            PreviousCell = PreviousRow.SubItems.Item(3)
            PreviousCellRow = PreviousRow
            PriceUpdate = True
            MEUpdate = False
        End If

    End Sub

    ' Shows the text box on the grid where clicked if enabled
    Private Sub ShowEditBox(ListRef As ListView)

        ' Save the center location of the edit box
        SavedListClickLoc.X = CurrentCell.Bounds.Left + CInt(CurrentCell.Bounds.Width / 2)
        SavedListClickLoc.Y = CurrentCell.Bounds.Top + CInt(CurrentCell.Bounds.Height / 2)

        ' Get the boundry data for the control now
        Dim pTop As Integer = ListRef.Top + CurrentCell.Bounds.Top
        Dim pLeft As Integer = ListRef.Left + CurrentCell.Bounds.Left + 2 ' pad right by 2 to align better
        Dim CurrentParent As Control

        CurrentParent = ListRef.Parent
        ' Look up all locations of parent controls to get the location for the control boundaries when shown
        Do Until CurrentParent.Name = "frmMain"
            pTop = pTop + CurrentParent.Top
            pLeft = pLeft + CurrentParent.Left
            CurrentParent = CurrentParent.Parent
        Loop

        If ListRef.Name <> lstRawPriceProfile.Name And ListRef.Name <> lstManufacturedPriceProfile.Name Or PriceModifierUpdate Then
            With txtListEdit
                .Hide()
                ' Set the bounds of the control
                .SetBounds(pLeft, pTop, CurrentCell.Bounds.Width, CurrentCell.Bounds.Height)
                .Text = CurrentCell.Text
                .Show()
                If CurrentRow.SubItems(2).Text = txtListEdit.Text Then
                    .TextAlign = HorizontalAlignment.Center
                Else
                    .TextAlign = HorizontalAlignment.Right
                End If

                .Focus()
            End With
            cmbEdit.Visible = False
        Else ' updates on the price profile grids

            Dim rsData As SQLiteDataReader
            Dim SQL As String = ""

            With cmbEdit
                UpdatingCombo = True

                If PriceRegionUpdate Then
                    Call LoadRegionCombo(cmbEdit, CurrentCell.Text)
                    ' Set the bounds of the control
                    .SetBounds(pLeft, pTop, CurrentCell.Bounds.Width, CurrentCell.Bounds.Height)
                    .Show()
                    .Focus()
                Else
                    .Hide()
                    .BeginUpdate()
                    .Items.Clear()
                    If PriceSystemUpdate Then
                        ' Base it off the data in the region cell
                        SQL = "SELECT solarSystemName FROM SOLAR_SYSTEMS, REGIONS "
                        SQL = SQL & "WHERE SOLAR_SYSTEMS.regionID = REGIONS.regionID "
                        SQL = SQL & "AND REGIONS.regionName = '" & PreviousCell.Text & "' "
                        SQL = SQL & "ORDER BY solarSystemName"
                        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                        rsData = DBCommand.ExecuteReader

                        ' Add all systems if it's the system
                        .Items.Add(AllSystems)
                        While rsData.Read
                            .Items.Add(rsData.GetString(0))
                        End While
                    ElseIf PriceTypeUpdate Then
                        ' Manually enter these
                        .Items.Add("Min Sell")
                        .Items.Add("Max Sell")
                        .Items.Add("Avg Sell")
                        .Items.Add("Median Sell")
                        .Items.Add("Percentile Sell")
                        .Items.Add("Min Buy")
                        .Items.Add("Max Buy")
                        .Items.Add("Avg Buy")
                        .Items.Add("Median Buy")
                        .Items.Add("Percentile Buy")
                        .Items.Add("Min Buy & Sell")
                        .Items.Add("Max Buy & Sell")
                        .Items.Add("Avg Buy & Sell")
                        .Items.Add("Median Buy & Sell")
                        .Items.Add("Percentile Buy & Sell")
                    End If

                    ' Set the bounds of the control
                    .SetBounds(pLeft, pTop, CurrentCell.Bounds.Width, CurrentCell.Bounds.Height)
                    .Text = CurrentCell.Text
                    .EndUpdate()
                    .Show()
                    .Focus()
                End If
                DataEntered = False ' We just updated so reset
                UpdatingCombo = False
            End With
            txtListEdit.Visible = False
        End If
    End Sub

    ' Processes the tab function in the text box for the grid. This overrides the default tabbing between controls
    Protected Overrides Function ProcessTabKey(ByVal TabForward As Boolean) As Boolean
        Dim ac As Control = Me.ActiveControl

        TabPressed = True

        If TabForward Then
            If ac Is txtListEdit Or ac Is cmbEdit Then
                Call ProcessKeyDownEdit(Keys.Tab, SelectedGrid)
                Return True
            End If
        Else
            If ac Is txtListEdit Or ac Is cmbEdit Then
                ' This is Shift + Tab but just send Shift for ease of processing
                Call ProcessKeyDownEdit(Keys.ShiftKey, SelectedGrid)
                Return True
            End If
        End If

        Return MyBase.ProcessTabKey(TabForward)

    End Function

    Private Sub cmbEdit_DropDownClosed(sender As Object, e As System.EventArgs) Handles cmbEdit.DropDownClosed
        If (PriceRegionUpdate And cmbEdit.Text <> PreviousRegion) Or
            (PriceSystemUpdate And cmbEdit.Text <> PreviousSystem) Or
            (PriceTypeUpdate And cmbEdit.Text <> PreviousPriceType) And Not UpdatingCombo Then
            DataEntered = True
            Call ProcessKeyDownEdit(Keys.Enter, SelectedGrid)
        End If
    End Sub

    Private Sub cmbEdit_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbEdit.SelectedIndexChanged
        If Not DataUpdated Then
            DataEntered = True
        End If
    End Sub

    Private Sub cmbEdit_LostFocus(sender As Object, e As System.EventArgs) Handles cmbEdit.LostFocus
        ' Lost focus some other way than tabbing
        If ((PriceRegionUpdate And cmbEdit.Text <> PreviousRegion) Or
            (PriceSystemUpdate And cmbEdit.Text <> PreviousSystem) Or
            (PriceTypeUpdate And cmbEdit.Text <> PreviousPriceType)) _
            And Not TabPressed And Not UpdatingCombo Then
            DataEntered = True
            Call ProcessKeyDownEdit(Keys.Enter, SelectedGrid)
        End If
        cmbEdit.Visible = False
        TabPressed = False
    End Sub

    Private Sub txtListEdit_GotFocus(sender As Object, e As System.EventArgs) Handles txtListEdit.GotFocus
        Call txtListEdit.SelectAll()
    End Sub

    Private Sub txtListEdit_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtListEdit.KeyDown
        If Not DataEntered Then ' If data already entered, then they didn't do it through paste
            DataEntered = ProcessCutCopyPasteSelect(txtListEdit, e)
        End If

        If e.KeyCode = Keys.Enter Then
            IgnoreFocus = True
            Call ProcessKeyDownEdit(Keys.Enter, SelectedGrid)
            IgnoreFocus = False
        End If
    End Sub

    Private Sub txtListEdit_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtListEdit.KeyPress
        ' Make sure it's the right format for ME or Price update
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If MEUpdate Then
                If allowedMETEChars.IndexOf(e.KeyChar) = -1 Then
                    ' Invalid Character
                    e.Handled = True
                Else
                    DataEntered = True
                End If
            ElseIf PriceUpdate Then
                If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                    ' Invalid Character
                    e.Handled = True
                Else
                    DataEntered = True
                End If
            ElseIf PriceModifierUpdate Then
                If allowedNegativePercentChars.IndexOf(e.KeyChar) = -1 Then
                    ' Invalid Character
                    e.Handled = True
                Else
                    DataEntered = True
                End If
            End If

        End If
    End Sub

    Private Sub txtListEdit_LostFocus(sender As Object, e As System.EventArgs) Handles txtListEdit.LostFocus
        If Not RefreshingGrid And DataEntered And Not IgnoreFocus And (PriceModifierUpdate And txtListEdit.Text <> PreviousPriceMod) Then
            Call ProcessKeyDownEdit(Keys.Enter, SelectedGrid)
        End If
        txtListEdit.Visible = False
    End Sub

    Private Sub txtListEdit_TextChanged(sender As Object, e As System.EventArgs) Handles txtListEdit.TextChanged
        If MEUpdate Then ' make sure they only enter 0-10 for values
            Call VerifyMETEEntry(txtListEdit, "ME")
        End If
    End Sub

    ' Sets the variables for price profiles
    Private Sub SetPriceProfileVariables(Index As Integer)
        PriceTypeUpdate = False
        PriceRegionUpdate = False
        PriceSystemUpdate = False
        PriceModifierUpdate = False

        Select Case Index
            Case 1
                PriceTypeUpdate = True
                PreviousPriceType = CurrentCell.Text
            Case 2
                PriceRegionUpdate = True
                PreviousRegion = CurrentCell.Text
            Case 3
                PriceSystemUpdate = True
                PreviousSystem = CurrentCell.Text
            Case 4
                PriceModifierUpdate = True
                PreviousPriceMod = CurrentCell.Text
        End Select

    End Sub

    ' Detects Scroll event and hides boxes
    Private Sub lstBPComponentMats_ProcMsg(ByVal m As System.Windows.Forms.Message) Handles lstBPComponentMats.ProcMsg
        txtListEdit.Hide()
        cmbEdit.Hide()
    End Sub

    ' Detects Scroll event and hides boxes
    Private Sub lstBPRawMats_ProcMsg(ByVal m As System.Windows.Forms.Message) Handles lstBPRawMats.ProcMsg
        txtListEdit.Hide()
        cmbEdit.Hide()
    End Sub

    ' Detects Scroll event and hides boxes
    Private Sub lstPricesView_ProcMsg(ByVal m As System.Windows.Forms.Message) Handles lstPricesView.ProcMsg
        txtListEdit.Hide()
        cmbEdit.Hide()
    End Sub

    ' Detects Scroll event and hides boxes
    Private Sub lstRawPriceProfile_ProcMsg(ByVal m As System.Windows.Forms.Message) Handles lstRawPriceProfile.ProcMsg
        txtListEdit.Hide()
        cmbEdit.Hide()
    End Sub

    ' Detects Scroll event and hides boxes
    Private Sub lstManufacturedPriceProfile_ProcMsg(ByVal m As System.Windows.Forms.Message) Handles lstManufacturedPriceProfile.ProcMsg
        txtListEdit.Hide()
        cmbEdit.Hide()
    End Sub

#End Region

#Region "Blueprints Tab"

#Region "Blueprints Tab User Objects (Check boxes, Text, Buttons) Functions/Procedures "

    Private Sub chkPerUnit_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPPricePerUnit.CheckedChanged
        If Not FirstLoad And Not UpdatingCheck Then
            Call UpdateBPPriceLabels()
            ' Update history too
            Call UpdateBPHistory(False)
        End If
    End Sub

    Private Sub txtBPLines_DoubleClick(sender As Object, e As System.EventArgs) Handles txtBPLines.DoubleClick
        ' Enter the max lines we have
        txtBPLines.Text = CStr(MaximumProductionLines)
    End Sub

    Private Sub txtBPLines_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtBPLines.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPLines, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPLines_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPLines.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtBPInventionLines_DoubleClick(sender As Object, e As System.EventArgs) Handles txtBPInventionLines.DoubleClick
        ' Enter the max lines we have
        txtBPInventionLines.Text = CStr(MaximumLaboratoryLines)
    End Sub

    Private Sub txtBPInventionLines_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtBPInventionLines.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPInventionLines, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPInventionLines_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPInventionLines.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtBPRelicLines_DoubleClick(sender As Object, e As System.EventArgs)
        ' Enter the max lines we have
        txtBPRelicLines.Text = CStr(MaximumLaboratoryLines)
    End Sub

    Private Sub txtBPRelicLines_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs)
        Call ProcessCutCopyPasteSelect(txtBPRelicLines, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPRelicLines_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs)
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub lblBPCanMakeBP_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPCanMakeBP.DoubleClick
        ' Only allow if items in list
        If lstBPComponentMats.Items.Count > 0 Then
            Dim f1 As New frmReqSkills(SkillType.BPReqSkills)
            f1.Show()
        End If
    End Sub

    Private Sub lblBPCanMakeBPAll_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPCanMakeBPAll.DoubleClick
        ' Don't allow popup if buying all
        If lblBPCanMakeBPAll.Text = "Buying all Materials" Then
            Exit Sub
        End If

        ' Only update the make all label if we have something to make, else use the bp data
        If SelectedBlueprint.HasComponents Then
            Dim f1 As New frmReqSkills(SkillType.BPComponentSkills)
            f1.Show()
        Else
            Dim f1 As New frmReqSkills(SkillType.BPReqSkills)
            f1.Show()
        End If
    End Sub

    Private Sub lblBPInventStatus_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblBPT2InventStatus.DoubleClick
        Dim f1 As New frmReqSkills(SkillType.InventionReqSkills)
        f1.Show()
    End Sub

    Private Sub lblReverseEngineerStatus_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblT3InventStatus.DoubleClick
        Dim f1 As New frmReqSkills(SkillType.REReqSkills)
        f1.Show()
    End Sub

    Private Sub txtBPCCosts_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub chkBPBuildBuy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPBuildBuy.CheckedChanged
        ' Disable the choice for raw or components for shopping list and just add components
        If Not FirstLoad And Not UpdatingCheck Then
            If chkBPBuildBuy.Checked Then
                rbtnBPComponentCopy.Enabled = True
                rbtnBPRawmatCopy.Enabled = False
            Else
                If Not IsNothing(SelectedBlueprint) Then
                    If SelectedBlueprint.HasComponents Then
                        rbtnBPComponentCopy.Enabled = True
                        rbtnBPRawmatCopy.Enabled = True
                    Else
                        rbtnBPComponentCopy.Enabled = False
                        rbtnBPRawmatCopy.Enabled = False
                    End If
                End If
            End If

            ' Refresh
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
            End If

        End If

    End Sub

    Private Sub lstBPComponentMats_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstBPComponentMats.ColumnClick
        Call ListViewColumnSorter(e.Column, CType(lstBPComponentMats, ListView), BPCompColumnClicked, BPCompColumnSortType)
    End Sub

    Private Sub lstBPRawMats_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstBPRawMats.ColumnClick
        Call ListViewColumnSorter(e.Column, CType(lstBPRawMats, ListView), BPRawColumnClicked, BPRawColumnSortType)
    End Sub

    Private Sub ResetInventionBoxes()
        ' Reset Decrytpor
        ResetDecryptorCombos(0)

        lblBPInventionCost.Text = "0.00"
        lblBPRECost.Text = "0.00"
        lblBPInventionChance.Text = "0%"
        lblBPDecryptorStats.Text = "ME: 0, TE: 0," & vbCrLf & "BP Runs: 0"

    End Sub

    Private Sub ResetDecryptorCombos(InventionTech As Integer)
        LoadingInventionDecryptors = True
        LoadingT3Decryptors = True
        InventionDecryptorsLoaded = False
        T3DecryptorsLoaded = False

        Dim TempDecryptors As New DecryptorList

        ' Auto load the decryptor if they want
        If InventionTech = 2 Then
            If UserApplicationSettings.SaveBPRelicsDecryptors And UserBPTabSettings.T2DecryptorType <> "" Then
                cmbBPInventionDecryptor.Text = UserBPTabSettings.T2DecryptorType
                SelectedDecryptor = TempDecryptors.GetDecryptor(cmbBPInventionDecryptor.Text)
            Else
                cmbBPInventionDecryptor.Text = None
                SelectedDecryptor = NoDecryptor ' Reset the selected decryptor too
            End If
        ElseIf InventionTech = 3 Then
            ' Load for T3
            If UserApplicationSettings.SaveBPRelicsDecryptors And UserBPTabSettings.T3DecryptorType <> "" Then
                cmbBPT3Decryptor.Text = UserBPTabSettings.T3DecryptorType
                SelectedDecryptor = TempDecryptors.GetDecryptor(cmbBPT3Decryptor.Text)
            Else
                cmbBPT3Decryptor.Text = None
                SelectedDecryptor = NoDecryptor ' Reset the selected decryptor too
            End If

            ' Reset both
        Else
            cmbBPInventionDecryptor.Text = None
            cmbBPT3Decryptor.Text = None
            SelectedDecryptor = NoDecryptor ' Reset the selected decryptor too
        End If

        LoadingInventionDecryptors = False
        LoadingT3Decryptors = False

    End Sub

    Private Sub ResetfromTechSizeCheck()
        cmbBPsLoaded = False

        ComboMenuDown = False
        MouseWheelSelection = False
        ComboBoxArrowKeys = False
        BPComboKeyDown = False

        Call LoadBlueprintCombo()

        cmbBPBlueprintSelection.Text = "Select Blueprint"
        cmbBPBlueprintSelection.Focus()

    End Sub

    Private Sub btnReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearItemFilter.Click
        txtPriceItemFilter.Text = ""
        Call UpdatePriceList()
    End Sub

    Private Sub txtBPRuns_GotFocus(sender As Object, e As System.EventArgs) Handles txtBPRuns.GotFocus
        Call txtBPRuns.SelectAll()
    End Sub

    Private Sub txtBPRuns_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtBPRuns.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPRuns, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPRuns_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPRuns.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtBPRuns_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtBPRuns.KeyUp
        If Not EnterKeyPressed Then
            EnterKeyPressed = False
        End If
    End Sub

    Private Sub txtBPRuns_LostFocus(sender As Object, e As System.EventArgs) Handles txtBPRuns.LostFocus
        If Not IgnoreFocus Then
            Call UpdateBPLinesandBPs()
            IgnoreFocus = True
        End If
    End Sub

    Private Sub txtBPAddlCosts_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtBPAddlCosts.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPAddlCosts, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPAddlCosts_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPAddlCosts.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtBPAddlCosts_LostFocus(sender As Object, e As System.EventArgs) Handles txtBPAddlCosts.LostFocus
        If IsNumeric(txtBPAddlCosts.Text) Then
            txtBPAddlCosts.Text = FormatNumber(txtBPAddlCosts.Text, 2)
        ElseIf Trim(txtBPAddlCosts.Text) = "" Then
            txtBPAddlCosts.Text = "0.00"
        End If
    End Sub

    Private Sub txtBPME_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtBPME.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPME, e)
        If e.KeyCode = Keys.Enter Then
            Call EnterKeyRunBP(e)
        End If
    End Sub

    Private Sub txtBPME_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPME.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedMETEChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtBPME_TextChanged(sender As Object, e As System.EventArgs) Handles txtBPME.TextChanged
        Call VerifyMETEEntry(txtBPME, "ME")
    End Sub

    Private Sub txtBPME_LostFocus(sender As Object, e As System.EventArgs) Handles txtBPME.LostFocus
        If Trim(txtBPME.Text) = "" Then
            txtBPME.Text = "0"
        End If
    End Sub

    Private Sub txtBPTE_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtBPTE.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPTE, e)
        If e.KeyCode = Keys.Enter Then
            Call EnterKeyRunBP(e)
        End If
    End Sub

    Private Sub txtBPTE_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPTE.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedMETEChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtBPTE_TextChanged(sender As Object, e As System.EventArgs) Handles txtBPTE.TextChanged
        Call VerifyMETEEntry(txtBPTE, "TE")
    End Sub

    Private Sub txtBPTE_LostFocus(sender As Object, e As System.EventArgs) Handles txtBPTE.LostFocus
        If Trim(txtBPTE.Text) = "" Then
            txtBPTE.Text = "0"
        End If
    End Sub

    Private Sub chkBPFacilityIncludeUsage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'If Not FirstLoad And Not ChangingUsageChecks And Not SentFromManufacturingTab Then
        '    If Not IsNothing(SelectedBlueprint) And Not SentFromManufacturingTab Then
        '        Call SetDefaultFacilitybyCheck(GetProductionType(cmbBPFacilityActivities.Text, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, cmbBPFacilityType.Text),
        '                chkBPFacilityIncludeUsage, BPTab, cmbBPFacilityType.Text, cmbBPFacilityorArray,
        '                lblBPFacilityDefault, btnBPFacilitySave, Nothing, Nothing, ttBP)

        '        Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID)
        '    End If
        'End If
    End Sub

    Private Sub chkBPTaxesFees_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPTaxes.CheckedChanged
        If Not FirstLoad And SetTaxFeeChecks Then
            If Not IsNothing(SelectedBlueprint) Then
                Call SelectedBlueprint.SetPriceData(chkBPTaxes.Checked, chkBPBrokerFees.Checked)
                Call UpdateBPPriceLabels()
            End If
        End If
    End Sub

    Private Sub chkBPBrokerFees_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPBrokerFees.CheckedChanged
        If Not FirstLoad And SetTaxFeeChecks Then
            If Not IsNothing(SelectedBlueprint) Then
                Call SelectedBlueprint.SetPriceData(chkBPTaxes.Checked, chkBPBrokerFees.Checked)
                Call UpdateBPPriceLabels()
            End If
        End If
    End Sub

    Private Sub txtBPNumBPs_DoubleClick(sender As Object, e As System.EventArgs) Handles txtBPNumBPs.DoubleClick
        If Not IsNothing(SelectedBlueprint) Then
            txtBPNumBPs.Text = CStr(GetUsedNumBPs(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, CInt(txtBPRuns.Text),
                                                  CInt(txtBPLines.Text), CInt(txtBPNumBPs.Text), SelectedDecryptor.RunMod))
        End If
    End Sub

    Private Sub txtBPNumBPs_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtBPNumBPs.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPNumBPs, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPNumBPs_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPNumBPs.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub cmbBPInventionDecryptor_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbBPInventionDecryptor.DropDown
        Call LoadBPInventionDecryptors()
    End Sub

    Private Sub LoadBPInventionDecryptors()
        If Not InventionDecryptorsLoaded Then
            ' Clear anything that was there
            cmbBPInventionDecryptor.Items.Clear()

            ' Add NONE
            cmbBPInventionDecryptor.Items.Add(None)

            Dim Decryptors As New DecryptorList

            For i = 0 To Decryptors.GetDecryptorList.Count - 1
                cmbBPInventionDecryptor.Items.Add(Decryptors.GetDecryptorList(i).Name)
            Next

            InventionDecryptorsLoaded = True

        End If
    End Sub

    Private Sub LoadBPT3InventionDecryptors()
        If Not T3DecryptorsLoaded Then
            ' Clear anything that was there
            cmbBPT3Decryptor.Items.Clear()

            ' Add NONE
            cmbBPT3Decryptor.Items.Add(None)

            Dim Decryptors As New DecryptorList

            For i = 0 To Decryptors.GetDecryptorList.Count - 1
                cmbBPT3Decryptor.Items.Add(Decryptors.GetDecryptorList(i).Name)
            Next

            T3DecryptorsLoaded = True

        End If
    End Sub

    Private Sub cmbBPInventionDecryptor_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbBPInventionDecryptor.SelectedIndexChanged

        ' Only load when the user selects a new decryptor from the list, not when changing the text
        If Not LoadingInventionDecryptors Then
            Call SelectDecryptor(cmbBPInventionDecryptor.Text)

            ' Reload the number of bps you need etc
            ' If the runs changed, update the lines data based on decryptor, need to update it first before running
            Call UpdateBPLinesandBPs()

            ' Use the original ME and TE values when they change the decryptor
            Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)

        End If

    End Sub

    Private Sub cmbBPT3Decryptor_DropDown(sender As Object, e As System.EventArgs) Handles cmbBPT3Decryptor.DropDown
        Call LoadBPT3InventionDecryptors()
    End Sub

    Private Sub cmbBPREDecryptor_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbBPT3Decryptor.SelectedIndexChanged

        ' Only load when the user selects a new decryptor from the list, not when changing the text
        If Not LoadingT3Decryptors Then
            Call SelectDecryptor(cmbBPT3Decryptor.Text)

            ' Reload the number of bps you need etc
            ' If the runs changed, update the lines data based on decryptor, need to update it first before running
            Call UpdateBPLinesandBPs()

            ' Use the original ME and TE values when they change the decryptor
            Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)

        End If
    End Sub

    Private Sub cmbBPRelic_DropDown(sender As Object, e As System.EventArgs) Handles cmbBPRelic.DropDown

        If Not RelicsLoaded Then
            Call LoadRelicTypes(SelectedBlueprint.GetTypeID)
        End If

    End Sub

    Private Sub cmbBPRelic_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbBPRelic.SelectedIndexChanged

        If Not LoadingRelics Then
            ' Use the original values when selecting a new relic
            Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
        End If

    End Sub

    Private Sub SetInventionEnabled(InventionType As String, Enable As Boolean)
        If InventionType = "T2" Then
            chkBPIncludeCopyCosts.Enabled = Enable
            chkBPIncludeCopyTime.Enabled = Enable
            chkBPIncludeInventionCosts.Enabled = Enable
            chkBPIncludeInventionTime.Enabled = Enable

            txtBPInventionLines.Enabled = Enable
            cmbBPInventionDecryptor.Enabled = Enable
        Else
            chkBPIncludeT3Costs.Enabled = Enable
            chkBPIncludeT3Time.Enabled = Enable
            txtBPRelicLines.Enabled = Enable
            cmbBPT3Decryptor.Enabled = Enable
            cmbBPRelic.Enabled = Enable
        End If
    End Sub

    Private Sub chkBPIgnoreInvention_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIgnoreInvention.CheckedChanged
        UpdatingInventionChecks = True

        If chkBPIgnoreInvention.Checked Then
            If tabBPInventionEquip.Contains(tabInventionCalcs) Then
                ' Disable all first
                Call SetInventionEnabled("T2", False)
                Call BPTabFacility.SetIgnoreInvention(True, ProductionType.Invention, False)

            ElseIf tabBPInventionEquip.Contains(tabT3Calcs) Then
                ' Disable all first
                Call SetInventionEnabled("T3", False)
                Call BPTabFacility.SetIgnoreInvention(True, ProductionType.T3Invention, False)
            End If

            ' In both cases, disable the num bps box
            txtBPNumBPs.Enabled = False

        Else ' Set it on the user settings
            If tabBPInventionEquip.Contains(tabInventionCalcs) Then
                ' Enable all first
                Call SetInventionEnabled("T2", True)
                Call BPTabFacility.SetIgnoreInvention(False, ProductionType.Invention, True)

            ElseIf tabBPInventionEquip.Contains(tabT3Calcs) Then
                ' Enable all first
                Call SetInventionEnabled("T3", True)
                Call BPTabFacility.SetIgnoreInvention(False, ProductionType.T3Invention, True)
            End If

            txtBPNumBPs.Enabled = True

        End If

        UpdatingInventionChecks = False

        ' If we are inventing, make sure we add or remove the activity based on the check
        If tabBPInventionEquip.Contains(tabInventionCalcs) Or tabBPInventionEquip.Contains(tabT3Calcs) Then
            If chkBPIgnoreInvention.Checked Then
                txtBPME.Enabled = True
                txtBPTE.Enabled = True
            Else
                txtBPME.Enabled = False
                txtBPTE.Enabled = False
            End If
        End If

        If Not FirstLoad Then
            Call RefreshBP()
        End If

    End Sub

    Private Sub chkBPIgnoreMinerals_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIgnoreMinerals.CheckedChanged
        If Not FirstLoad Then
            Call RefreshBP()
        End If
    End Sub

    Private Sub chkBPIgnoreT1Item_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPIgnoreT1Item.CheckedChanged
        If Not FirstLoad Then
            Call RefreshBP()
        End If
    End Sub

    ' Loads the T3 Relic types into the combo box based on BP Selected
    Private Sub LoadRelicTypes(ByVal BPID As Long)
        Dim SQL As String
        Dim readerRelic As SQLiteDataReader
        Dim RelicName As String
        Dim UserRelicType As String = ""

        LoadingRelics = True

        SQL = "SELECT typeName FROM INVENTORY_TYPES, INDUSTRY_ACTIVITY_PRODUCTS WHERE productTypeID =" & BPID & " "
        SQL = SQL & "AND typeID = blueprintTypeID AND activityID = 8"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerRelic = DBCommand.ExecuteReader

        If UserBPTabSettings.RelicType <> "" Then
            If UserBPTabSettings.RelicType.Contains(WreckedRelic) Then
                UserRelicType = WreckedRelic
            ElseIf UserBPTabSettings.RelicType.Contains(MalfunctioningRelic) Then
                UserRelicType = MalfunctioningRelic
            ElseIf UserBPTabSettings.RelicType.Contains(IntactRelic) Then
                UserRelicType = IntactRelic
            End If
        End If

        cmbBPRelic.Items.Clear()

        While readerRelic.Read
            RelicName = readerRelic.GetString(0)
            cmbBPRelic.Items.Add(RelicName)
            ' Load the name of the Wrecked Relic or base tactical destroyer relic in the combo when found 
            If RelicName.Contains(WreckedRelic) And UserBPTabSettings.RelicType = "" Then
                cmbBPRelic.Text = RelicName
            ElseIf UserRelicType <> "" Then
                If RelicName.Contains(UserRelicType) Then
                    cmbBPRelic.Text = RelicName
                End If
            End If
        End While

        readerRelic.Close()

        readerRelic = Nothing
        DBCommand = Nothing

        LoadingRelics = False
        RelicsLoaded = True

    End Sub

    Private Sub btnCopyMatstoClip_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBPCopyMatstoClip.Click
        Dim ClipboardData = New DataObject
        Dim OutputText As String
        Dim DecryptorText As String = ""
        Dim RelicText As String = ""
        Dim AddlText As String = ""

        If chkBPSimpleCopy.Checked = False Then
            If cmbBPInventionDecryptor.Text <> None Then
                DecryptorText = "Decryptor: " & cmbBPInventionDecryptor.Text
            End If

            If cmbBPRelic.Text <> None Then
                RelicText = "Relic: " & cmbBPRelic.Text
            End If

            If RelicText <> "" Then
                AddlText = ", " & RelicText
            Else
                ' Decryptor
                If DecryptorText <> "" Then
                    AddlText = ", " & DecryptorText
                End If
            End If

            AddlText = ")" & Environment.NewLine & Environment.NewLine

            If rbtnBPRawmatCopy.Checked Or chkBPBuildBuy.Checked Then
                OutputText = "Raw Material List for " & txtBPRuns.Text & " Units of '" & cmbBPBlueprintSelection.Text & "' (ME: " & CStr(txtBPME.Text) & AddlText
                OutputText = OutputText & SelectedBlueprint.GetRawMaterials.GetClipboardList(UserApplicationSettings.DataExportFormat, False, False, False, UserApplicationSettings.IncludeInGameLinksinCopyText)
            Else
                OutputText = "Component Material List for " & txtBPRuns.Text & " Units of '" & cmbBPBlueprintSelection.Text & "' (ME: " & CStr(txtBPME.Text) & AddlText
                OutputText = OutputText & SelectedBlueprint.GetComponentMaterials.GetClipboardList(UserApplicationSettings.DataExportFormat, False, False, False, UserApplicationSettings.IncludeInGameLinksinCopyText)
            End If

            If UserApplicationSettings.ShopListIncludeInventMats Then
                If Not IsNothing(SelectedBlueprint.GetInventionMaterials.GetMaterialList) Then
                    OutputText = OutputText & Environment.NewLine & Environment.NewLine & "Invention Materials" & Environment.NewLine & Environment.NewLine
                    OutputText = OutputText & SelectedBlueprint.GetInventionMaterials.GetClipboardList(UserApplicationSettings.DataExportFormat, False, False, False, UserApplicationSettings.IncludeInGameLinksinCopyText)
                End If
            End If
        Else
            ' Just copy the materials for use in evepraisal etc.
            OutputText = ""
            If (chkBPBuildBuy.Checked And rbtnBPCopyInvREMats.Checked = False) Or rbtnBPRawmatCopy.Checked Then
                For i = 0 To SelectedBlueprint.GetRawMaterials.GetMaterialList.Count - 1
                    OutputText += String.Format("{0} {1}{2}", SelectedBlueprint.GetRawMaterials.GetMaterialList(i).GetMaterialName(), SelectedBlueprint.GetRawMaterials.GetMaterialList(i).GetQuantity(), vbCrLf)
                Next
            ElseIf rbtnBPComponentCopy.Checked And rbtnBPCopyInvREMats.Checked = False Then
                For i = 0 To SelectedBlueprint.GetComponentMaterials.GetMaterialList.Count - 1
                    OutputText += String.Format("{0} {1}{2}", SelectedBlueprint.GetComponentMaterials.GetMaterialList(i).GetMaterialName(), SelectedBlueprint.GetComponentMaterials.GetMaterialList(i).GetQuantity(), vbCrLf)
                Next
            End If

            If UserApplicationSettings.ShopListIncludeInventMats Or rbtnBPCopyInvREMats.Checked Then
                If Not IsNothing(SelectedBlueprint.GetInventionMaterials.GetMaterialList) Then
                    For i = 0 To SelectedBlueprint.GetInventionMaterials.GetMaterialList.Count - 1
                        OutputText += String.Format("{0} {1}{2}", SelectedBlueprint.GetInventionMaterials.GetMaterialList(i).GetMaterialName(), SelectedBlueprint.GetInventionMaterials.GetMaterialList(i).GetQuantity(), vbCrLf)
                    Next
                End If
            End If

        End If

        ' Paste to clipboard
        Call CopyTextToClipboard(OutputText)

    End Sub

    Private Sub cmbBPInventionDecryptor_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles cmbBPInventionDecryptor.KeyPress
        e.Handled = True
    End Sub

    Private Sub cmbBPREDecryptor_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles cmbBPT3Decryptor.KeyPress
        e.Handled = True
    End Sub

    Private Sub rbtnAllBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPAllBlueprints.CheckedChanged
        If rbtnBPAllBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, True, True, True, True)
        End If
    End Sub

    Private Sub rbtnBPOwnedBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPOwnedBlueprints.CheckedChanged
        If rbtnBPOwnedBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, True, True, True, True)
        End If
    End Sub

    'Private Sub chkBPIncludeIgnoredBPs_CheckedChanged(sender As System.Object, e As System.EventArgs)
    '    If chkBPIncludeIgnoredBPs.Checked Then
    '        Call ResetBlueprintCombo(True, True, True, True, True, True)
    '    End If
    'End Sub

    Private Sub rbtnShipBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPShipBlueprints.CheckedChanged
        If rbtnBPShipBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, True, False, True, True)
        End If
    End Sub

    Private Sub rbtnModuleBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPModuleBlueprints.CheckedChanged
        If rbtnBPModuleBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, False, True, True, False)
        End If
    End Sub

    Private Sub rbtnDroneBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPDroneBlueprints.CheckedChanged
        If rbtnBPDroneBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, False, False, False, True)
        End If
    End Sub

    Private Sub rbtnComponentBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPComponentBlueprints.CheckedChanged
        If rbtnBPComponentBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnSubsystemBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPSubsystemBlueprints.CheckedChanged
        If rbtnBPSubsystemBlueprints.Checked Then
            Call ResetBlueprintCombo(False, False, True, False, False, False)
        End If
    End Sub

    Private Sub rbtnToolBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPMiscBlueprints.CheckedChanged
        If rbtnBPMiscBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnAmmoChargeBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPAmmoChargeBlueprints.CheckedChanged
        If rbtnBPAmmoChargeBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnRigBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPRigBlueprints.CheckedChanged
        If rbtnBPRigBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnStructureBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPStructureBlueprints.CheckedChanged
        If rbtnBPStructureBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, True)
        End If
    End Sub

    Private Sub rbtnBoosterBlueprints_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnBPBoosterBlueprints.CheckedChanged
        If rbtnBPBoosterBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnBPDeployableBlueprints_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbtnBPDeployableBlueprints.CheckedChanged
        If rbtnBPDeployableBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnBPStationPartsBlueprints_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbtnBPStructureRigsBlueprints.CheckedChanged
        If rbtnBPStructureRigsBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnBPStationModulesBlueprints_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnBPStructureModulesBlueprints.CheckedChanged
        If rbtnBPStructureModulesBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnBPReactionsBlueprints_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnBPReactionsBlueprints.CheckedChanged
        If rbtnBPReactionsBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnBPCelestialBlueprints_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbtnBPCelestialsBlueprints.CheckedChanged
        If rbtnBPCelestialsBlueprints.Checked Then
            Call ResetBlueprintCombo(True, False, False, False, False, False)
        End If
    End Sub

    Private Sub rbtnBPFavoriteBlueprints_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbtnBPFavoriteBlueprints.CheckedChanged
        If rbtnBPFavoriteBlueprints.Checked Then
            Call ResetBlueprintCombo(True, True, True, True, True, True)
        End If
    End Sub

    Private Sub chkbpT1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPT1.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkbpT2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPT2.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkbpT3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPT3.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPNavyFaction_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPNavyFaction.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPPirateFaction_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPPirateFaction.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPStoryline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBPStoryline.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPSmall_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPSmall.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPMedium_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPMedium.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPLarge_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPLarge.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub chkBPXL_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPXL.CheckedChanged
        If Not FirstLoad Then
            Call ResetfromTechSizeCheck()
        End If
    End Sub

    Private Sub btnRefreshBP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBPRefreshBP.Click
        Call RefreshBP()
    End Sub

    Public Sub RefreshBP(Optional IgnoreFocus As Boolean = False)
        If CorrectMETE(txtBPME.Text, txtBPTE.Text, txtBPME, txtBPTE) Then
            If Not IsNothing(SelectedBlueprint) Then
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
                txtBPRuns.SelectAll()
                If Not IgnoreFocus Then
                    txtBPRuns.Focus()
                End If
            End If
        End If
    End Sub

    Private Sub lstBPComponentMats_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles lstBPComponentMats.MouseClick
        Call ListClicked(lstBPComponentMats, sender, e)
    End Sub

    Private Sub lstBPRawMats_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles lstBPRawMats.MouseClick
        Call ListClicked(lstBPRawMats, sender, e)
    End Sub

    Private Sub EnterKeyRunBP(ByVal e As System.Windows.Forms.KeyEventArgs)
        If CorrectMETE(txtBPME.Text, txtBPTE.Text, txtBPME, txtBPTE) Then
            If e.KeyCode = Keys.Enter Then
                EnterKeyPressed = True
                Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)
                txtBPRuns.SelectAll()
                IgnoreFocus = True
                txtBPRuns.Focus()
                IgnoreFocus = False
            End If
        End If
    End Sub

    Private Sub btnBPBack_Click(sender As System.Object, e As System.EventArgs) Handles btnBPBack.Click
        Call LoadPreviousBlueprint()
    End Sub

    Private Sub btnBPForward_Click(sender As System.Object, e As System.EventArgs) Handles btnBPForward.Click
        Call LoadNextBlueprint()
    End Sub

    Private Sub tabBPInventionEquip_Click(sender As System.Object, e As System.EventArgs) Handles tabBPInventionEquip.Click
        SelectedBPTabIndex = tabBPInventionEquip.SelectedIndex
    End Sub

    Private Sub txtBPUpdateCostIndex_GotFocus(sender As Object, e As System.EventArgs) Handles txtBPUpdateCostIndex.GotFocus
        txtBPUpdateCostIndex.SelectAll()
    End Sub

    Private Sub txtBPUpdateCostIndex_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtBPUpdateCostIndex.KeyDown
        Call ProcessCutCopyPasteSelect(txtBPUpdateCostIndex, e)
        Call EnterKeyRunBP(e)
    End Sub

    Private Sub txtBPUpdateCostIndex_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPUpdateCostIndex.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPercentChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                btnBPUpdateCostIndex.Enabled = True
            End If
        End If
    End Sub

    Private Sub txtBPUpdateCostIndex_LostFocus(sender As Object, e As System.EventArgs) Handles txtBPUpdateCostIndex.LostFocus
        If IsNumeric(txtBPAddlCosts.Text) Then
            txtBPAddlCosts.Text = FormatNumber(txtBPAddlCosts.Text, 2)
        ElseIf Trim(txtBPAddlCosts.Text) = "" Then
            txtBPAddlCosts.Text = "0.00"
        End If
    End Sub

    Private Sub txtBPMarketPriceEdit_GotFocus(sender As Object, e As System.EventArgs) Handles txtBPMarketPriceEdit.GotFocus
        txtBPMarketPriceEdit.SelectAll()
    End Sub

    Private Sub txtBPMarketPriceEdit_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtBPMarketPriceEdit.KeyDown
        If Not DataEntered Then ' If data already entered, then they didn't do it through paste
            DataEntered = ProcessCutCopyPasteSelect(txtBPMarketPriceEdit, e)
        End If

        If e.KeyCode = Keys.Enter Then
            IgnoreMarketFocus = True
            ' Update the price for this item
            Call UpdateMarketPriceManually()
            IgnoreMarketFocus = False
        End If
    End Sub

    Private Sub txtBPMarketPriceEdit_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtBPMarketPriceEdit.KeyPress
        ' Make sure it's the right format for Price update
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If

    End Sub

    Private Sub txtBPMarketPriceEdit_LostFocus(sender As Object, e As System.EventArgs) Handles txtBPMarketPriceEdit.LostFocus
        If Not IgnoreMarketFocus And txtBPMarketPriceEdit.Text <> lblBPMarketCost.Text Then
            Call UpdateMarketPriceManually()
        End If
        txtBPMarketPriceEdit.Visible = False
    End Sub

    Private Sub UpdateMarketPriceManually()

        Dim SQL As String = "UPDATE ITEM_PRICES SET PRICE = " & CStr(CDbl(txtBPMarketPriceEdit.Text)) & ", PRICE_TYPE = 'User' WHERE ITEM_ID = " & SelectedBlueprint.GetItemID
        Call EVEDB.ExecuteNonQuerySQL(SQL)
        Call PlayNotifySound()
        lblBPMarketCost.Text = FormatNumber(txtBPMarketPriceEdit.Text, 2)
        IgnoreFocus = True
        Call RefreshBP()
    End Sub

    Private Sub lblBPBPSVR_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPBPSVR.DoubleClick
        lblBPBPSVR.Text = GetBPItemSVR(SelectedBlueprint.GetProductionTime) ' just bp time
    End Sub

    Private Sub lblBPRawSVR_DoubleClick(sender As Object, e As System.EventArgs) Handles lblBPRawSVR.DoubleClick
        lblBPRawSVR.Text = GetBPItemSVR(SelectedBlueprint.GetTotalProductionTime) ' total time to build everything
    End Sub

    Private Sub lblBPCompProfit_DoubleClick(sender As System.Object, e As System.EventArgs) Handles lblBPCompProfit.DoubleClick
        If lblBPCompProfit1.Text.Contains("Percent") Then
            ' Swap to profit
            lblBPCompProfit.Text = FormatNumber(SelectedBlueprint.GetTotalComponentProfit, 2)
            lblBPCompProfit1.Text = "Component Profit:"
        Else
            lblBPCompProfit.Text = FormatPercent(SelectedBlueprint.GetTotalComponentProfitPercent, 2)
            lblBPCompProfit1.Text = "Component Profit Percent:"
        End If
    End Sub

    Private Sub lblBPRawProfit_DoubleClick(sender As System.Object, e As System.EventArgs) Handles lblBPRawProfit.DoubleClick
        If lblBPRawProfit1.Text.Contains("Percent") Then
            ' Swap to profit
            lblBPRawProfit.Text = FormatNumber(SelectedBlueprint.GetTotalRawProfit, 2)
            lblBPRawProfit1.Text = "Raw Profit:"
        Else
            lblBPRawProfit.Text = FormatPercent(SelectedBlueprint.GetTotalRawProfitPercent, 2)
            lblBPRawProfit1.Text = "Raw Profit Percent:"
        End If
    End Sub

    Private Sub lstBPComponentMats_DoubleClick(sender As Object, e As System.EventArgs) Handles lstBPComponentMats.DoubleClick

        ' If the item doesn't have an ME (set to "-") then don't load
        If lstBPComponentMats.SelectedItems(0).SubItems(2).Text <> "-" Then
            Dim rsBP As SQLiteDataReader
            Dim SQL As String
            Dim BuildType As String = ""

            SQL = "SELECT BLUEPRINT_ID, PORTION_SIZE, ITEM_GROUP_ID, ITEM_CATEGORY_ID, BLUEPRINT_NAME    FROM ALL_BLUEPRINTS WHERE ITEM_NAME ="
            SQL = SQL & "'" & lstBPComponentMats.SelectedItems(0).SubItems(0).Text & "'"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            rsBP = DBCommand.ExecuteReader()
            rsBP.Read()

            If chkBPBuildBuy.Checked Then
                BuildType = "Build/Buy"
            End If

            ' Adjust the runs for porition size needed and use that instead
            Dim BPID As Long = rsBP.GetInt64(0)
            Dim Runs As Long = CLng(Math.Ceiling(CLng(lstBPComponentMats.SelectedItems(0).SubItems(1).Text) / rsBP.GetInt32(1)))
            Dim GroupID As Integer = rsBP.GetInt32(2)
            Dim CategoryID As Integer = rsBP.GetInt32(3)
            Dim BPName As String = rsBP.GetString(4
                                                  )
            rsBP.Close()

            Dim SelectedActivity As String = ""
            If BPName.Contains("Reaction Formula") Then
                SelectedActivity = ManufacturingFacility.ActivityReactions
            Else
                SelectedActivity = ManufacturingFacility.ActivityManufacturing
            End If

            With BPTabFacility
                Call LoadBPfromEvent(BPID, BuildType, "Raw", SentFromLocation.BlueprintTab,
                                    .GetSelectedManufacturingFacility(GroupID, CategoryID, SelectedActivity), .GetFacility(ProductionType.ComponentManufacturing),
                                    .GetFacility(ProductionType.CapitalComponentManufacturing),
                                    .GetSelectedInventionFacility(GroupID, CategoryID), .GetFacility(ProductionType.Copying),
                                    chkBPTaxes.Checked, chkBPBrokerFees.Checked,
                                    lstBPComponentMats.SelectedItems(0).SubItems(2).Text, txtBPTE.Text,
                                    CStr(Runs), txtBPLines.Text, txtBPInventionLines.Text,
                                    "1", txtBPAddlCosts.Text, chkBPPricePerUnit.Checked) ' Use 1 bp for now
            End With
        End If

    End Sub

    Private Sub lstPricesView_ColumnWidthChanging(sender As Object, e As System.Windows.Forms.ColumnWidthChangingEventArgs) Handles lstPricesView.ColumnWidthChanging
        If e.ColumnIndex = 0 Or e.ColumnIndex >= 4 Then
            e.Cancel = True
            e.NewWidth = lstPricesView.Columns(e.ColumnIndex).Width
        End If
    End Sub

    ' Makes sure we have a tech checked for blueprints
    Private Sub EnsureBPTechCheck()
        If chkBPT1.Enabled And chkBPT1.Checked Then
            Exit Sub
        ElseIf chkBPT2.Enabled And chkBPT2.Checked Then
            Exit Sub
        ElseIf chkBPT3.Enabled And chkBPT3.Checked Then
            Exit Sub
        ElseIf chkBPNavyFaction.Enabled And chkBPNavyFaction.Checked Then
            Exit Sub
        ElseIf chkBPPirateFaction.Enabled And chkBPPirateFaction.Checked Then
            Exit Sub
        ElseIf chkBPStoryline.Enabled And chkBPStoryline.Checked Then
            Exit Sub
        End If

        ' If here, then none are checked that are enabled, find the first one enabled and check it
        If chkBPT1.Enabled Then
            chkBPT1.Checked = True
            Exit Sub
        ElseIf chkBPT2.Enabled Then
            chkBPT2.Checked = True
            Exit Sub
        ElseIf chkBPT3.Enabled Then
            chkBPT3.Checked = True
            Exit Sub
        ElseIf chkBPNavyFaction.Enabled Then
            chkBPNavyFaction.Checked = True
            Exit Sub
        ElseIf chkBPPirateFaction.Enabled Then
            chkBPPirateFaction.Checked = True
            Exit Sub
        ElseIf chkBPStoryline.Enabled Then
            chkBPStoryline.Checked = True
            Exit Sub
        End If

    End Sub

#End Region

#Region "BP Combo / List Processing "

    Private Sub cmbBPBlueprintSelection_DropDown(sender As Object, e As System.EventArgs) Handles cmbBPBlueprintSelection.DropDown
        ' If you drop down, don't show the text window
        'cmbBPBlueprintSelection.AutoCompleteMode = AutoCompleteMode.None
        lstBPList.Hide()
        ComboMenuDown = True
        ' if we drop down, we aren't using the arrow keys
        ComboBoxArrowKeys = False
        BPComboKeyDown = False
    End Sub

    Private Sub cmbBPBlueprintSelection_DropDownClosed(sender As Object, e As System.EventArgs) Handles cmbBPBlueprintSelection.DropDownClosed
        ' If it closes up, re-enable autocomplete
        'cmbBPBlueprintSelection.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        ComboMenuDown = False
        lstBPList.Hide() ' This could show up if people type into the list when combo down
        'Call SelectBlueprint() ' Loads in selectionchangecommitted
        cmbBPBlueprintSelection.Focus()
    End Sub

    Private Sub cmbBPBlueprintSelection_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles cmbBPBlueprintSelection.MouseWheel
        ' Only set mouse boolean when the combo isn't dropped down since users might want to use the wheel and click to select
        If ComboMenuDown Then
            MouseWheelSelection = False
        Else
            MouseWheelSelection = True
            cmbBPBlueprintSelection.Focus()
        End If

    End Sub

    Private Sub cmbBPBlueprintSelection_DoubleClick(sender As Object, e As EventArgs) Handles cmbBPBlueprintSelection.DoubleClick
        cmbBPBlueprintSelection.SelectAll()
    End Sub

    Private Sub cmbBPBlueprintSelection_LostFocus(sender As Object, e As EventArgs) Handles cmbBPBlueprintSelection.LostFocus
        ' Close the list view when lost focus
        Call lstBPList.Hide()
        Call cmbBPBlueprintSelection.SelectAll()
    End Sub

    ' Thrown when the user changes the value in the combo box
    Private Sub cmbBPBlueprintSelection_SelectionChangeCommitted(sender As Object, e As System.EventArgs) Handles cmbBPBlueprintSelection.SelectionChangeCommitted

        If Not MouseWheelSelection And Not ComboBoxArrowKeys Then
            lstBPList.Visible = False ' We are loading the bp, so hide this
            BPSelected = True
            Call LoadBPFromCombo()
        End If

        BPSelected = False

    End Sub

    ' Load the list box when the user types and don't use the drop down list
    Private Sub cmbBPBlueprintSelection_TextChanged(sender As System.Object, e As System.EventArgs) Handles cmbBPBlueprintSelection.TextChanged
        If Not FirstLoad And Not BPSelected And Trim(cmbBPBlueprintSelection.Text) <> "Select Blueprint" And BPComboKeyDown Then
            If ComboBoxArrowKeys = False Then
                If (cmbBPBlueprintSelection.Text <> "") Then
                    GetBPWithName(cmbBPBlueprintSelection.Text)
                End If
                If (String.IsNullOrEmpty(cmbBPBlueprintSelection.Text)) Then
                    lstBPList.Items.Clear()
                    lstBPList.Visible = False
                End If
            End If
        End If
    End Sub

    ' Process keys for bp combo
    Private Sub cmbBPBlueprintSelection_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles cmbBPBlueprintSelection.KeyDown

        If cmbBPBlueprintSelection.DroppedDown = False Then
            BPComboKeyDown = True
        Else
            BPComboKeyDown = False
        End If

        If e.KeyValue = Keys.Up Or e.KeyValue = Keys.Down Then
            ComboBoxArrowKeys = True
        Else
            ComboBoxArrowKeys = False
        End If

        ' If they hit the arrow keys when the combo is dropped down (just in the combo it won't throw this)
        If lstBPList.Visible = False Then
            ' If they select enter, then load the BP
            If e.KeyValue = Keys.Enter Then
                Call LoadBPFromCombo()
            End If
        Else
            ' They have the list down, so process up and down keys to work with selecting in the list
            e.Handled = True ' Don't process up and down in the combo when the list shown
            Select Case (e.KeyCode)
                Case Keys.Down
                    If (lstBPList.SelectedIndex < lstBPList.Items.Count - 1) Then
                        lstBPList.SelectedIndex = lstBPList.SelectedIndex + 1
                    End If
                Case Keys.Up
                    If (lstBPList.SelectedIndex > 0) Then
                        lstBPList.SelectedIndex = lstBPList.SelectedIndex - 1
                    End If
                Case Keys.Enter
                    If (lstBPList.SelectedIndex > -1) Then
                        cmbBPBlueprintSelection.Text = lstBPList.SelectedItem.ToString()
                        lstBPList.Visible = False
                        BPSelected = True
                        Call SelectBlueprint()
                        BPSelected = False
                    End If
                Case Keys.Escape
                    lstBPList.Visible = False
            End Select
        End If

    End Sub

    Private Sub cmbBlueprintSelection_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call cmbBPBlueprintSelection.SelectAll()
    End Sub

    ' Process up down arrows in bp list
    Private Sub lstBPList_SelectedValueChanged(sender As Object, e As EventArgs) Handles lstBPList.SelectedValueChanged
        If Not IsNothing(lstBPList.SelectedItem) Then
            cmbBPBlueprintSelection.Text = lstBPList.SelectedItem.ToString
            cmbBPBlueprintSelection.SelectAll()
        End If
    End Sub

    ' Loads the item by clicking on the item selected
    Private Sub lstBPList_MouseDown(sender As Object, e As MouseEventArgs) Handles lstBPList.MouseDown
        If lstBPList.SelectedItems.Count <> 0 Then
            cmbBPBlueprintSelection.Text = lstBPList.SelectedItem.ToString()
            lstBPList.Visible = False
            Call SelectBlueprint()
            cmbBPBlueprintSelection.Focus()
        End If
    End Sub

    Private Sub lstBPList_MouseMove(sender As Object, e As MouseEventArgs) Handles lstBPList.MouseMove
        Dim Index As Integer = lstBPList.IndexFromPoint(e.X, e.Y)

        RemoveHandler lstBPList.SelectedValueChanged, AddressOf lstBPList_SelectedValueChanged
        lstBPList.SelectedIndex = Index
        AddHandler lstBPList.SelectedValueChanged, AddressOf lstBPList_SelectedValueChanged
    End Sub

    Private Sub lstBPList_LostFocus(sender As Object, e As EventArgs) Handles lstBPList.LostFocus
        ' hide when losing focus
        Call lstBPList.Hide()
        Call cmbBPBlueprintSelection.SelectAll()
    End Sub

    ' Loads the blueprint combo based on what was selected
    Private Sub LoadBlueprintCombo()
        Dim readerBPs As SQLiteDataReader
        Dim SQL As String = ""

        Application.UseWaitCursor = True
        If Not cmbBPsLoaded Then
            ' Clear anything that was there
            cmbBPBlueprintSelection.Items.Clear()
            cmbBPBlueprintSelection.BeginUpdate()

            ' Core Query ' Get rid of 's in blueprint name for sorting
            If Me.rbtnBPOwnedBlueprints.Checked Or Me.rbtnBPFavoriteBlueprints.Checked Then
                SQL = BuildBPSelectQuery()
            Else
                SQL = "SELECT ALL_BLUEPRINTS.BLUEPRINT_NAME, REPLACE(LOWER(BLUEPRINT_NAME),'''','') AS X FROM ALL_BLUEPRINTS, INVENTORY_TYPES "
                SQL = SQL & "WHERE ALL_BLUEPRINTS.ITEM_ID = INVENTORY_TYPES.typeID "
                SQL = SQL & BuildBPSelectQuery()
            End If

            If SQL = "" Then
                Application.UseWaitCursor = False
                Exit Sub
            End If

            SQL = SQL & " ORDER BY X"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerBPs = DBCommand.ExecuteReader

            While readerBPs.Read
                ' Add the data to the array and combo
                cmbBPBlueprintSelection.Items.Add(Trim(readerBPs.GetString(0)))
                Application.DoEvents()
            End While

            readerBPs.Close()

            readerBPs = Nothing
            DBCommand = Nothing

            cmbBPBlueprintSelection.EndUpdate()

            cmbBPsLoaded = True

        End If
        Application.UseWaitCursor = False

    End Sub

    Private Sub GetBPWithName(bpName As String)
        ' Query: SELECT BLUEPRINT_NAME AS bpName FROM ALL_BLUEPRINTS b, INVENTORY_TYPES t WHERE b.ITEM_ID = t.typeID AND bpName LIKE '%Repair%'
        Dim readerBP As SQLiteDataReader
        Dim query As String

        cmbBPBlueprintSelection.Text = bpName
        lstBPList.Items.Clear()

        ' Add limiting functions here based on radio buttons
        ' Use replace to Get rid of 's in blueprint name for sorting
        If Me.rbtnBPOwnedBlueprints.Checked Or Me.rbtnBPFavoriteBlueprints.Checked Then
            query = BuildBPSelectQuery()
        Else
            query = "SELECT ALL_BLUEPRINTS.BLUEPRINT_NAME, REPLACE(LOWER(BLUEPRINT_NAME),'''','') AS X FROM ALL_BLUEPRINTS, INVENTORY_TYPES "
            query = query & "WHERE ALL_BLUEPRINTS.ITEM_ID = INVENTORY_TYPES.typeID "
            query = query & BuildBPSelectQuery()
            query = query & " AND ALL_BLUEPRINTS.BLUEPRINT_NAME LIKE '%" & FormatDBString(bpName) & "%'"
            query = query & " ORDER BY X"
        End If

        ' query = "SELECT BLUEPRINT_NAME AS bpName FROM ALL_BLUEPRINTS b, INVENTORY_TYPES t WHERE b.ITEM_ID = t.typeID AND bpName LIKE '%" & bpName & "%'"

        DBCommand = New SQLiteCommand(query, EVEDB.DBREf)
        readerBP = DBCommand.ExecuteReader
        lstBPList.BeginUpdate()

        While readerBP.Read()
            lstBPList.Items.Add(readerBP.GetString(0))
            Application.DoEvents()
        End While

        readerBP.Close()
        readerBP = Nothing
        lstBPList.EndUpdate()
        lstBPList.Visible = True
        Application.UseWaitCursor = False

    End Sub

    ' Builds the query for the select combo
    Private Function BuildBPSelectQuery() As String
        Dim SQL As String = ""
        Dim SQLItemType As String = ""

        ' Find what type of blueprint we want
        With Me
            If .rbtnBPAmmoChargeBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY = 'Charge' "
            ElseIf .rbtnBPDroneBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY in ('Drone', 'Fighter') "
            ElseIf .rbtnBPModuleBlueprints.Checked Then
                SQL = SQL & "AND (ITEM_CATEGORY ='Module' AND ITEM_GROUP NOT LIKE 'Rig%') "
            ElseIf .rbtnBPShipBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY = 'Ship' "
            ElseIf .rbtnBPSubsystemBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY = 'Subsystem' "
            ElseIf .rbtnBPBoosterBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY = 'Implant' "
            ElseIf .rbtnBPComponentBlueprints.Checked Then
                SQL = SQL & "AND (ITEM_GROUP LIKE '%Components%' AND ITEM_GROUP <> 'Station Components') "
            ElseIf .rbtnBPMiscBlueprints.Checked Then
                SQL = SQL & "AND ITEM_GROUP IN ('Tool','Data Interfaces','Cyberimplant','Fuel Block') "
            ElseIf .rbtnBPDeployableBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY = 'Deployable' "
            ElseIf .rbtnBPCelestialsBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY IN ('Celestial','Orbitals','Sovereignty Structures', 'Station', 'Accessories', 'Infrastructure Upgrades') "
            ElseIf .rbtnBPStructureBlueprints.Checked Then
                SQL = SQL & "AND (ITEM_CATEGORY IN ('Starbase','Structure') OR ITEM_GROUP = 'Station Components') "
            ElseIf .rbtnBPStructureRigsBlueprints.Checked Then
                SQL = SQL & "AND ITEM_CATEGORY = 'Structure Rigs' "
            ElseIf .rbtnBPStructureModulesBlueprints.Checked Then
                SQL = SQL & "AND (ITEM_CATEGORY = 'Structure Module' AND BLUEPRINT_GROUP NOT LIKE '%Rig Blueprint') "
            ElseIf .rbtnBPReactionsBlueprints.Checked Then
                SQL = SQL & "AND BLUEPRINT_GROUP LIKE '%Reaction Formulas'"
            ElseIf .rbtnBPRigBlueprints.Checked Then
                SQL = SQL & "AND BLUEPRINT_GROUP LIKE '%Rig Blueprint' "
            ElseIf .rbtnBPOwnedBlueprints.Checked Then
                SQL = "SELECT ALL_BLUEPRINTS.BLUEPRINT_NAME, REPLACE(LOWER(ALL_BLUEPRINTS.BLUEPRINT_NAME),'''','') AS X FROM ALL_BLUEPRINTS, INVENTORY_TYPES, "
                SQL = SQL & "OWNED_BLUEPRINTS WHERE OWNED <> 0 "
                SQL = SQL & "AND OWNED_BLUEPRINTS.USER_ID IN (" & SelectedCharacter.ID & "," & SelectedCharacter.CharacterCorporation.CorporationID & ") "
                SQL = SQL & "AND ALL_BLUEPRINTS.BLUEPRINT_ID = OWNED_BLUEPRINTS.BLUEPRINT_ID "
                SQL = SQL & "AND ALL_BLUEPRINTS.ITEM_ID = INVENTORY_TYPES.typeID "
            ElseIf .rbtnBPFavoriteBlueprints.Checked Then
                SQL = "Select ALL_BLUEPRINTS.BLUEPRINT_NAME, REPLACE(LOWER(ALL_BLUEPRINTS.BLUEPRINT_NAME),'''','') AS X FROM ALL_BLUEPRINTS, INVENTORY_TYPES, "
                SQL = SQL & "OWNED_BLUEPRINTS WHERE OWNED <> 0 "
                SQL = SQL & "AND OWNED_BLUEPRINTS.USER_ID IN (" & SelectedCharacter.ID & "," & SelectedCharacter.CharacterCorporation.CorporationID & ") "
                SQL = SQL & "AND ALL_BLUEPRINTS.BLUEPRINT_ID = OWNED_BLUEPRINTS.BLUEPRINT_ID AND FAVORITE = 1 "
                SQL = SQL & "AND ALL_BLUEPRINTS.ITEM_ID = INVENTORY_TYPES.typeID "
            End If
        End With

        ' Item Type Definitions - These are set by me based on existing data
        ' 1, 2, 14 are T1, T2, T3
        ' 3 is Storyline
        ' 15 is Pirate Faction
        ' 16 is Navy Faction

        ' Check Tech version
        If chkBPT1.Enabled Then
            ' Only a Subsystem so T3
            If chkBPT1.Checked Then
                SQLItemType = SQLItemType & "1,"
            End If
        End If

        If chkBPT2.Enabled Then
            If chkBPT2.Checked Then
                SQLItemType = SQLItemType & "2,"
            End If
        End If

        If chkBPT3.Enabled Then
            If chkBPT3.Checked Then
                SQLItemType = SQLItemType & "14,"
            End If
        End If

        If chkBPStoryline.Enabled Then
            If chkBPStoryline.Checked Then
                SQLItemType = SQLItemType & "3,"
            End If
        End If

        If chkBPPirateFaction.Enabled Then
            If chkBPPirateFaction.Checked Then
                SQLItemType = SQLItemType & "15,"
            End If
        End If

        If chkBPNavyFaction.Enabled Then
            If chkBPNavyFaction.Checked Then
                SQLItemType = SQLItemType & "16,"
            End If
        End If

        ' Add Item Type
        If SQLItemType <> "" Then
            SQLItemType = " ALL_BLUEPRINTS.ITEM_TYPE IN (" & SQLItemType.Substring(0, SQLItemType.Length - 1) & ") "
        Else
            ' They need to have at least one. If not, just return nothing
            BuildBPSelectQuery = ""
            Exit Function
        End If

        ' Add the item types
        SQL = SQL & " AND " & SQLItemType

        Dim SizesClause As String = ""

        ' Finally add the sizes
        If chkBPSmall.Checked Then ' Light
            SizesClause = SizesClause & "'S',"
        End If

        If chkBPMedium.Checked Then ' Medium
            SizesClause = SizesClause & "'M',"
        End If

        If chkBPLarge.Checked Then ' Heavy
            SizesClause = SizesClause & "'L',"
        End If

        If chkBPXL.Checked Then ' Fighters
            SizesClause = SizesClause & "'XL',"
        End If

        If SizesClause <> "" Then
            SizesClause = " AND SIZE_GROUP IN (" & SizesClause.Substring(0, Len(SizesClause) - 1) & ") "
        End If

        SQL = SQL & SizesClause

        '' Ignore flag
        'If chkBPIncludeIgnoredBPs.Checked = False Then
        '    SQL = SQL & " AND IGNORE = 0 "
        'End If

        BuildBPSelectQuery = SQL

    End Function

    ' Loads a blueprint if selected in the combo box by different methods
    Private Sub LoadBPFromCombo()

        If Not IsNothing(cmbBPBlueprintSelection.SelectedItem) Then
            SelectedBPText = cmbBPBlueprintSelection.SelectedItem.ToString
            cmbBPBlueprintSelection.Text = SelectedBPText

            Call SelectBlueprint()

            ComboMenuDown = False
            MouseWheelSelection = False
            ComboBoxArrowKeys = False
            BPComboKeyDown = False

            SelectedBPText = ""
        End If

    End Sub

    ' Reloads the BP combo when run
    Private Sub ResetBlueprintCombo(ByVal T1 As Boolean, ByVal T2 As Boolean, ByVal T3 As Boolean, ByVal Storyline As Boolean, ByVal NavyFaction As Boolean, ByVal PirateFaction As Boolean)
        If Not FirstLoad Then
            cmbBPsLoaded = False
            chkBPT1.Enabled = T1
            chkBPT2.Enabled = T2
            chkBPT3.Enabled = T3
            chkBPNavyFaction.Enabled = NavyFaction
            chkBPPirateFaction.Enabled = PirateFaction
            chkBPStoryline.Enabled = Storyline

            ComboMenuDown = False
            MouseWheelSelection = False
            ComboBoxArrowKeys = False
            BPComboKeyDown = False

            ' Make sure we have something checked
            Call EnsureBPTechCheck()
            ' Load the New data
            Call LoadBlueprintCombo()

            cmbBPBlueprintSelection.Text = "Select Blueprint"
            cmbBPBlueprintSelection.Focus()
        End If
    End Sub

    Private Sub UpdateSelectedBPText(bpName As String)
        If bpName.Contains("Blueprint") Then
            RemoveHandler cmbBPBlueprintSelection.TextChanged, AddressOf cmbBPBlueprintSelection_TextChanged
            cmbBPBlueprintSelection.Text = bpName
            AddHandler cmbBPBlueprintSelection.TextChanged, AddressOf cmbBPBlueprintSelection_TextChanged
            Call SelectBlueprint()
        End If

    End Sub

    Private Sub btnBPListView_Click(sender As Object, e As EventArgs) Handles btnBPListView.Click
        Dim frmBPList = New frmBlueprintList
        AddHandler frmBPList.BPSelected, AddressOf UpdateSelectedBPText
        frmBPList.Show()

    End Sub

#End Region

    ' Initializes all the boxes on the BP tab
    Private Sub InitBPTab(Optional ResetBPHistory As Boolean = True)

        pictBP.Image = Nothing
        pictBP.BackgroundImage = Nothing
        pictBP.Update()

        cmbBPBlueprintSelection.Text = "Select Blueprint"

        With UserBPTabSettings
            ' Exort type (might change with build buy selection
            Select Case .ExporttoShoppingListType
                Case rbtnBPComponentCopy.Text
                    rbtnBPComponentCopy.Checked = True
                Case rbtnBPCopyInvREMats.Text
                    rbtnBPCopyInvREMats.Checked = True
                Case rbtnBPRawmatCopy.Text
                    rbtnBPRawmatCopy.Checked = True
            End Select

            ' Default build/buy
            chkBPBuildBuy.Checked = UserApplicationSettings.CheckBuildBuy

            cmbBPsLoaded = False
            InventionDecryptorsLoaded = False

            ' Set BP Lines to run production on
            txtBPNumBPs.Text = "1"

            ' Set the runs to 1
            txtBPRuns.Text = "1"

            ' Production time label
            lblBPProductionTime.Text = "00:00:00"
            lblBPTotalItemPT.Text = "00:00:00"

            ' SVR
            lblBPBPSVR.Text = "-"
            lblBPRawSVR.Text = "-"

            ' Cost labels
            lblBPRawMatCost.Text = "0.00"
            lblBPComponentMatCost.Text = "0.00"
            txtBPAddlCosts.Text = "0.00"

            ' Total
            lblBPRawTotalCost.Text = "0.00"
            lblBPTotalCompCost.Text = "0.00"

            lblBPRawIPH.Text = "0.00"
            lblBPRawIPH.ForeColor = Color.Black
            lblBPCompIPH.Text = "0.00"
            lblBPCompIPH.ForeColor = Color.Black

            lblBPCompProfit.Text = "0.00"
            lblBPCompProfit.ForeColor = Color.Black
            lblBPRawProfit.Text = "0.00"
            lblBPRawProfit.ForeColor = Color.Black

            lblBPMarketCost.Text = "0.00"

            ' Don't show labels to make
            lblBPCanMakeBP.Visible = False
            lblBPCanMakeBPAll.Visible = False

            ' Saved settings
            Select Case .BlueprintTypeSelection
                Case rbtnBPAllBlueprints.Text
                    rbtnBPAllBlueprints.Checked = True
                Case rbtnBPOwnedBlueprints.Text
                    rbtnBPOwnedBlueprints.Checked = True
                Case rbtnBPFavoriteBlueprints.Text
                    rbtnBPFavoriteBlueprints.Checked = True
                Case rbtnBPShipBlueprints.Text
                    rbtnBPShipBlueprints.Checked = True
                Case rbtnBPDroneBlueprints.Text
                    rbtnBPDroneBlueprints.Checked = True
                Case rbtnBPAmmoChargeBlueprints.Text
                    rbtnBPAmmoChargeBlueprints.Checked = True
                Case rbtnBPModuleBlueprints.Text
                    rbtnBPModuleBlueprints.Checked = True
                Case rbtnBPComponentBlueprints.Text
                    rbtnBPComponentBlueprints.Checked = True
                Case rbtnBPStructureBlueprints.Text
                    rbtnBPStructureBlueprints.Checked = True
                Case rbtnBPSubsystemBlueprints.Text
                    rbtnBPSubsystemBlueprints.Checked = True
                Case rbtnBPRigBlueprints.Text
                    rbtnBPRigBlueprints.Checked = True
                Case rbtnBPBoosterBlueprints.Text
                    rbtnBPBoosterBlueprints.Checked = True
                Case rbtnBPMiscBlueprints.Text
                    rbtnBPMiscBlueprints.Checked = True
                Case rbtnBPDeployableBlueprints.Text
                    rbtnBPDeployableBlueprints.Checked = True
                Case rbtnBPCelestialsBlueprints.Text
                    rbtnBPCelestialsBlueprints.Checked = True
                Case rbtnBPStructureRigsBlueprints.Text
                    rbtnBPStructureRigsBlueprints.Checked = True
                Case rbtnBPReactionsBlueprints.Text
                    rbtnBPReactionsBlueprints.Checked = True
            End Select

            chkBPT1.Checked = .Tech1Check
            chkBPT2.Checked = .Tech2Check
            chkBPT3.Checked = .Tech3Check
            chkBPNavyFaction.Checked = .TechFactionCheck
            chkBPStoryline.Checked = .TechStorylineCheck
            chkBPPirateFaction.Checked = .TechPirateCheck

            'chkBPIncludeIgnoredBPs.Checked = .IncludeIgnoredBPs
            chkBPSimpleCopy.Checked = .SimpleCopyCheck

            chkBPSmall.Checked = .SmallCheck
            chkBPMedium.Checked = .MediumCheck
            chkBPLarge.Checked = .LargeCheck
            chkBPXL.Checked = .XLCheck

            SetTaxFeeChecks = False
            'chkBPFacilityIncludeUsage.Checked = .IncludeUsage
            chkBPTaxes.Checked = .IncludeTaxes
            chkBPBrokerFees.Checked = .IncludeFees
            SetTaxFeeChecks = True

            chkBPPricePerUnit.Checked = .PricePerUnit

            BPRawColumnClicked = .RawColumnSort
            BPCompColumnClicked = .CompColumnSort

            If .RawColumnSortType = "Ascending" Then
                BPRawColumnSortType = SortOrder.Ascending
            Else
                BPRawColumnSortType = SortOrder.Descending
            End If

            If .CompColumnSortType = "Ascending" Then
                BPCompColumnSortType = SortOrder.Ascending
            Else
                BPCompColumnSortType = SortOrder.Descending
            End If

            ' Invention checks
            UpdatingInventionChecks = True
            chkBPIncludeInventionCosts.Checked = .IncludeInventionCost
            chkBPIncludeInventionTime.Checked = .IncludeInventionTime
            chkBPIncludeCopyCosts.Checked = .IncludeCopyCost
            chkBPIncludeCopyTime.Checked = .IncludeCopyTime
            chkBPIncludeT3Costs.Checked = .IncludeT3Cost
            chkBPIncludeT3Time.Checked = .IncludeT3Time
            UpdatingInventionChecks = False

            ' These facilities use the same include checks
            BPTabFacility.GetFacility(ProductionType.Invention).IncludeActivityCost = .IncludeInventionCost
            BPTabFacility.GetFacility(ProductionType.Invention).IncludeActivityTime = .IncludeInventionTime
            BPTabFacility.GetFacility(ProductionType.Copying).IncludeActivityCost = .IncludeCopyCost
            BPTabFacility.GetFacility(ProductionType.Copying).IncludeActivityTime = .IncludeCopyTime

            BPTabFacility.GetFacility(ProductionType.SubsystemManufacturing).IncludeActivityCost = .IncludeT3Cost
            BPTabFacility.GetFacility(ProductionType.SubsystemManufacturing).IncludeActivityTime = .IncludeT3Time
            BPTabFacility.GetFacility(ProductionType.T3CruiserManufacturing).IncludeActivityCost = .IncludeT3Cost
            BPTabFacility.GetFacility(ProductionType.T3CruiserManufacturing).IncludeActivityTime = .IncludeT3Time
            BPTabFacility.GetFacility(ProductionType.T3Invention).IncludeActivityCost = .IncludeT3Cost
            BPTabFacility.GetFacility(ProductionType.T3Invention).IncludeActivityTime = .IncludeT3Time

            ' Enter the max lines we have regardless
            txtBPLines.Text = CStr(.ProductionLines)
            ' Set Max Invention Lines
            txtBPInventionLines.Text = CStr(.LaboratoryLines)
            txtBPRelicLines.Text = CStr(.T3Lines)

            ' Ignore settings
            chkBPIgnoreInvention.Checked = .IgnoreInvention
            chkBPIgnoreMinerals.Checked = .IgnoreMinerals
            chkBPIgnoreT1Item.Checked = .IgnoreT1Item

            ' Profit labels
            If .RawProfitType = "Percent" Then
                lblBPRawProfit1.Text = "Raw Profit Percent:"
            Else
                lblBPRawProfit1.Text = "Raw Profit:"
            End If

            If .CompProfitType = "Percent" Then
                lblBPCompProfit1.Text = "Component Profit Percent:"
            Else
                lblBPCompProfit1.Text = "Component Profit:"
            End If

        End With

        ' Only show the facility and options tab first
        tabBPInventionEquip.TabPages.Remove(tabInventionCalcs)
        tabBPInventionEquip.TabPages.Remove(tabT3Calcs)
        tabBPInventionEquip.SelectTab(0)
        tabBPInventionEquip.Enabled = False

        ' Disable all entry areas until a blueprint is selected
        btnBPRefreshBP.Enabled = False
        btnBPCopyMatstoClip.Enabled = False
        btnBPAddBPMatstoShoppingList.Enabled = False
        chkBPSimpleCopy.Enabled = False
        txtBPME.Enabled = False
        txtBPTE.Enabled = False
        txtBPRuns.Enabled = False
        txtBPNumBPs.Enabled = False
        txtBPLines.Enabled = False
        chkBPPricePerUnit.Enabled = False
        txtBPAddlCosts.Enabled = False
        chkBPBuildBuy.Enabled = False
        chkBPTaxes.Enabled = False
        chkBPBrokerFees.Enabled = False
        gbBPManualSystemCostIndex.Enabled = False
        gbBPIgnoreinCalcs.Enabled = False

        ' Copy Labels
        rbtnBPComponentCopy.Enabled = False
        rbtnBPRawmatCopy.Enabled = False
        rbtnBPCopyInvREMats.Enabled = False

        ' Color Labels
        lblBPBuyColor.Visible = False
        lblBPBuildColor.Visible = False

        ' BP Combo selection booleans
        ComboMenuDown = False
        MouseWheelSelection = False
        ComboBoxArrowKeys = False
        BPComboKeyDown = False

        ' Clear grids
        lstBPComponentMats.Items.Clear()
        lstBPRawMats.Items.Clear()

        ResetBPTab = True
        EnterKeyPressed = False

        LoadingBPfromHistory = False

        ' Load the combo
        Call LoadBlueprintCombo()

        ' BP History
        If ResetBPHistory Then
            BPHistory = New List(Of BPHistoryItem)
            CurrentBPHistoryIndex = -1 ' Nothing added yet
            btnBPBack.Enabled = False
            btnBPForward.Enabled = False
        Else
            Call LoadBPfromHistory(CurrentBPHistoryIndex, "")
        End If

    End Sub

    ' Saves the settings on the form for default later
    Private Sub btnBPSaveSettings_Click(sender As System.Object, e As System.EventArgs) Handles btnBPSaveSettings.Click
        Dim TempSettings As BPTabSettings = Nothing
        Dim Settings As New ProgramSettings

        If Trim(txtBPLines.Text) <> "" Then
            If Not IsNumeric(txtBPLines.Text) Then
                MsgBox("Invalid BP Lines value", vbExclamation, Application.ProductName)
                txtBPLines.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtBPInventionLines.Text) <> "" Then
            If Not IsNumeric(txtBPInventionLines.Text) Then
                MsgBox("Invalid Invention Lines value", vbExclamation, Application.ProductName)
                txtBPInventionLines.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtBPRelicLines.Text) <> "" Then
            If Not IsNumeric(txtBPRelicLines.Text) Then
                MsgBox("Invalid T3 Invention Lines value", vbExclamation, Application.ProductName)
                txtBPRelicLines.Focus()
                Exit Sub
            End If
        End If

        With TempSettings
            ' Prod/Lab Lines
            .ProductionLines = CInt(txtBPLines.Text)
            .LaboratoryLines = CInt(txtBPInventionLines.Text)
            .T3Lines = CInt(txtBPRelicLines.Text)

            If rbtnBPAllBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPAllBlueprints.Text
            ElseIf rbtnBPOwnedBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPOwnedBlueprints.Text
            ElseIf rbtnBPFavoriteBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPFavoriteBlueprints.Text
            ElseIf rbtnBPShipBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPShipBlueprints.Text
            ElseIf rbtnBPDroneBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPDroneBlueprints.Text
            ElseIf rbtnBPAmmoChargeBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPAmmoChargeBlueprints.Text
            ElseIf rbtnBPModuleBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPModuleBlueprints.Text
            ElseIf rbtnBPComponentBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPComponentBlueprints.Text
            ElseIf rbtnBPStructureBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPStructureBlueprints.Text
            ElseIf rbtnBPSubsystemBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPSubsystemBlueprints.Text
            ElseIf rbtnBPRigBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPRigBlueprints.Text
            ElseIf rbtnBPBoosterBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPBoosterBlueprints.Text
            ElseIf rbtnBPMiscBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPMiscBlueprints.Text
            ElseIf rbtnBPCelestialsBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPCelestialsBlueprints.Text
            ElseIf rbtnBPDeployableBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPDeployableBlueprints.Text
            ElseIf rbtnBPStructureRigsBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPStructureRigsBlueprints.Text
            ElseIf rbtnBPReactionsBlueprints.Checked Then
                .BlueprintTypeSelection = rbtnBPReactionsBlueprints.Text
            End If

            If rbtnBPComponentCopy.Checked Then
                .ExporttoShoppingListType = rbtnBPComponentCopy.Text
            ElseIf rbtnBPRawmatCopy.Checked Then
                .ExporttoShoppingListType = rbtnBPRawmatCopy.Text
            ElseIf rbtnBPCopyInvREMats.Checked Then
                .ExporttoShoppingListType = rbtnBPCopyInvREMats.Text
            End If

            .Tech1Check = chkBPT1.Checked
            .Tech2Check = chkBPT2.Checked
            .Tech3Check = chkBPT3.Checked
            .TechStorylineCheck = chkBPStoryline.Checked
            .TechFactionCheck = chkBPNavyFaction.Checked
            .TechPirateCheck = chkBPPirateFaction.Checked

            ' .IncludeIgnoredBPs = chkBPIncludeIgnoredBPs.Checked
            .SimpleCopyCheck = chkBPSimpleCopy.Checked

            .SmallCheck = chkBPSmall.Checked
            .MediumCheck = chkBPMedium.Checked
            .LargeCheck = chkBPLarge.Checked
            .XLCheck = chkBPXL.Checked

            '.IncludeUsage = chkBPFacilityIncludeUsage.Checked
            .IncludeTaxes = chkBPTaxes.Checked
            .IncludeFees = chkBPBrokerFees.Checked

            .IncludeInventionCost = chkBPIncludeInventionCosts.Checked
            .IncludeInventionTime = chkBPIncludeInventionTime.Checked
            BPTabFacility.GetFacility(ProductionType.Invention).IncludeActivityCost = chkBPIncludeInventionCosts.Checked
            BPTabFacility.GetFacility(ProductionType.Invention).IncludeActivityTime = chkBPIncludeInventionTime.Checked

            .IncludeCopyCost = chkBPIncludeCopyCosts.Checked
            .IncludeCopyTime = chkBPIncludeCopyTime.Checked
            BPTabFacility.GetFacility(ProductionType.Copying).IncludeActivityCost = chkBPIncludeCopyCosts.Checked
            BPTabFacility.GetFacility(ProductionType.Copying).IncludeActivityTime = chkBPIncludeCopyTime.Checked

            ' For T3 on the BP tab, save both facility data
            .IncludeT3Cost = chkBPIncludeT3Costs.Checked
            .IncludeT3Time = chkBPIncludeT3Time.Checked

            ' Ignore settings
            .IgnoreInvention = chkBPIgnoreInvention.Checked
            .IgnoreMinerals = chkBPIgnoreMinerals.Checked
            .IgnoreT1Item = chkBPIgnoreT1Item.Checked

            BPTabFacility.GetFacility(ProductionType.SubsystemManufacturing).IncludeActivityCost = chkBPIncludeT3Costs.Checked
            BPTabFacility.GetFacility(ProductionType.SubsystemManufacturing).IncludeActivityTime = chkBPIncludeT3Time.Checked
            BPTabFacility.GetFacility(ProductionType.T3CruiserManufacturing).IncludeActivityCost = chkBPIncludeT3Costs.Checked
            BPTabFacility.GetFacility(ProductionType.T3CruiserManufacturing).IncludeActivityTime = chkBPIncludeT3Time.Checked
            BPTabFacility.GetFacility(ProductionType.T3Invention).IncludeActivityCost = chkBPIncludeT3Costs.Checked
            BPTabFacility.GetFacility(ProductionType.T3Invention).IncludeActivityTime = chkBPIncludeT3Time.Checked

            .PricePerUnit = chkBPPricePerUnit.Checked

            .CompColumnSort = BPCompColumnClicked
            .RawColumnSort = BPRawColumnClicked

            If BPCompColumnSortType = SortOrder.Ascending Then
                .CompColumnSortType = "Ascending"
            Else
                .CompColumnSortType = "Descending"
            End If

            If BPRawColumnSortType = SortOrder.Ascending Then
                .RawColumnSortType = "Ascending"
            Else
                .RawColumnSortType = "Descending"
            End If

            ' Save the relic and decryptor if they have the setting set
            If UserApplicationSettings.SaveBPRelicsDecryptors And Not IsNothing(SelectedBlueprint) Then
                ' See if the T2 window is open and has a decryptor then save, only will be open if they have a t2 bp loaded
                If SelectedBlueprint.GetTechLevel = BPTechLevel.T2 Then
                    .T2DecryptorType = cmbBPInventionDecryptor.Text
                    .RelicType = UserBPTabSettings.RelicType
                    .T3DecryptorType = UserBPTabSettings.T3DecryptorType ' Save the old one
                End If

                ' See if the T3 window is open and has a decryptor then save, only will be open if they have a t3 bp loaded
                If SelectedBlueprint.GetTechLevel = BPTechLevel.T3 Then
                    .T2DecryptorType = UserBPTabSettings.T2DecryptorType ' Save the old one
                    .RelicType = cmbBPRelic.Text
                    .T3DecryptorType = cmbBPT3Decryptor.Text
                End If
            End If

            ' Profit type
            If lblBPRawProfit1.Text.Contains("Percent") Then
                .RawProfitType = "Percent"
            Else
                .RawProfitType = "Profit"
            End If

            If lblBPCompProfit1.Text.Contains("Percent") Then
                .CompProfitType = "Percent"
            Else
                .CompProfitType = "Profit"
            End If

        End With

        ' Save these here too
        UserApplicationSettings.CheckBuildBuy = chkBPBuildBuy.Checked
        Call Settings.SaveApplicationSettings(UserApplicationSettings)

        ' Save the data in the XML file
        Call Settings.SaveBPSettings(TempSettings)

        ' Save the data to the local variable
        UserBPTabSettings = TempSettings

        MsgBox("Settings Saved", vbInformation, Application.ProductName)

    End Sub

    ' Saves the BP data
    Private Sub btnBPSaveBP_Click(sender As System.Object, e As System.EventArgs) Handles btnBPSaveBP.Click
        Dim AdditionalCost As Double
        Dim SaveBPType As BPType

        If IsNothing(SelectedBlueprint) Then
            Exit Sub
        End If

        ' Check additional costs for saving with this bp
        If IsNumeric(txtBPAddlCosts.Text) Then
            AdditionalCost = CDbl(txtBPAddlCosts.Text)
        Else
            AdditionalCost = 0
        End If

        ' Save the BP
        If CorrectMETE(txtBPME.Text, txtBPTE.Text, txtBPME, txtBPTE) Then
            If SelectedBlueprint.GetTechLevel = BPTechLevel.T2 And chkBPIgnoreInvention.Checked = True Then
                ' T2 BPO 
                SaveBPType = BPType.Original
            ElseIf SelectedBlueprint.GetTechLevel = BPTechLevel.T2 Or SelectedBlueprint.GetTechLevel = BPTechLevel.T3 Then
                ' Save T2/T3 an invented BPC, since if they aren't ignoring invention they have to use a decryptor or invention to get it
                SaveBPType = BPType.InventedBPC
            Else ' Everything else is a copy
                SaveBPType = BPType.Copy
            End If

            Call UpdateBPinDB(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetName, CInt(txtBPME.Text), CInt(txtBPTE.Text), SaveBPType,
                              CInt(txtBPME.Text), CInt(txtBPTE.Text), False, False, AdditionalCost)

            Call RefreshBP()

        End If

        MsgBox("BP Saved", vbInformation, Application.ProductName)

    End Sub

    ' Selects the blueprint from the combo and loads it into the grids
    Private Sub SelectBlueprint(Optional ByVal NewBP As Boolean = True, Optional SentFrom As SentFromLocation = 0)
        Dim SQL As String
        Dim readerBP As SQLiteDataReader
        Dim BPTypeID As Integer
        Dim TempTech As Integer
        Dim ItemType As Integer
        Dim ItemGroupID As Integer
        Dim ItemCategoryID As Integer
        Dim BPHasComponents As Boolean

        ' Set the number of runs to 1 if it's blank
        If Trim(txtBPRuns.Text) = "" Then
            txtBPRuns.Text = "1"
        End If

        ' Check the quantity
        If Not IsNumeric(txtBPRuns.Text) Then
            MsgBox("You must enter a valid number of runs", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPRuns.Focus()
            Exit Sub
        End If

        ' Check the num bps
        If Not IsNumeric(txtBPNumBPs.Text) Or Trim(txtBPNumBPs.Text) = "" Then
            MsgBox("You must enter a valid number of BPs", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPNumBPs.Focus()
            Exit Sub
        End If

        ' Additional costs
        If Not IsNumeric(txtBPAddlCosts.Text) Or Trim(txtBPAddlCosts.Text) = "" Then
            MsgBox("You must enter a valid additional cost value", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPAddlCosts.Focus()
            Exit Sub
        End If

        txtBPME.Enabled = True
        txtBPTE.Enabled = True

        SQL = "SELECT ALL_BLUEPRINTS.BLUEPRINT_ID, TECH_LEVEL, ITEM_TYPE, ITEM_GROUP_ID, ITEM_CATEGORY_ID "
        SQL = SQL & "FROM ALL_BLUEPRINTS "
        SQL = SQL & "WHERE ALL_BLUEPRINTS.BLUEPRINT_NAME = "

        If SelectedBPText = "" Then
            SelectedBPText = cmbBPBlueprintSelection.Text
        End If

        SQL = SQL & "'" & FormatDBString(SelectedBPText) & "'"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerBP = DBCommand.ExecuteReader

        If readerBP.Read() Then
            BPTypeID = readerBP.GetInt32(0)
            TempTech = readerBP.GetInt32(1)
            ItemType = readerBP.GetInt32(2)
            ItemGroupID = readerBP.GetInt32(3)
            ItemCategoryID = readerBP.GetInt32(4)
        Else
            Exit Sub
        End If

        readerBP.Close()

        ' See if this has buildable components
        BPHasComponents = DoesBPHaveBuildableComponents(BPTypeID)

        ' Load the facilty based on the groupid and categoryid
        Call BPTabFacility.LoadFacility(ItemGroupID, ItemCategoryID, TempTech, BPHasComponents)

        ' Load the image
        Call LoadBlueprintPicture(BPTypeID, ItemType)

        ' Set for max production lines - bp tab or history (bp tab)
        If SentFrom = SentFromLocation.History Or SentFrom = SentFromLocation.BlueprintTab Then ' We might have different values there and they set on double click
            ' Reset the entry boxes
            txtBPRuns.Text = "1"
            txtBPNumBPs.Text = "1"

            txtBPLines.Text = CStr(UserBPTabSettings.ProductionLines)
            txtBPInventionLines.Text = CStr(UserBPTabSettings.LaboratoryLines)
            txtBPRelicLines.Text = CStr(UserBPTabSettings.T3Lines)

            Call ResetDecryptorCombos(TempTech)

        Else ' Sent from manufacturing tab or shopping list
            ' Set up for Reloading the decryptor combo on T2/T3
            ' Allow reloading of Decryptors
            InventionDecryptorsLoaded = False
            T3DecryptorsLoaded = False
            If TempTech = 2 Then
                cmbBPInventionDecryptor.Text = SelectedDecryptor.Name
            Else
                cmbBPT3Decryptor.Text = SelectedDecryptor.Name
            End If
            ' Allow loading decryptors on drop down
            LoadingInventionDecryptors = False
            LoadingT3Decryptors = False
            ' Allow reloading of relics
            RelicsLoaded = False
        End If

        ' Finally set the ME and TE in the display (need to allow the user to choose different BP's and play with ME/TE) - Search user bps first
        SQL = "SELECT ME, TE, ADDITIONAL_COSTS, RUNS, BP_TYPE"
        SQL = SQL & " FROM OWNED_BLUEPRINTS WHERE USER_ID =" & SelectedCharacter.ID
        SQL = SQL & " AND BLUEPRINT_ID = " & BPTypeID & " AND OWNED <> 0 " ' Only load user or api owned bps

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerBP = DBCommand.ExecuteReader()

        Dim HasOwnedBP As Boolean = False

        If readerBP.Read() Then
            HasOwnedBP = True
        Else
            ' Try again with corp
            readerBP.Close()
            SQL = "SELECT ME, TE, ADDITIONAL_COSTS, RUNS, BP_TYPE"
            SQL = SQL & " FROM OWNED_BLUEPRINTS WHERE USER_ID =" & SelectedCharacter.CharacterCorporation.CorporationID
            SQL = SQL & " AND BLUEPRINT_ID = " & BPTypeID & " AND SCANNED <> 0 AND OWNED <> 0 "

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerBP = DBCommand.ExecuteReader()

            If readerBP.Read() Then
                HasOwnedBP = True
            End If
        End If

        Dim OwnedBPRuns As Integer

        If HasOwnedBP And Not SentFrom = SentFromLocation.ManufacturingTab Then
            txtBPME.Text = CStr(readerBP.GetInt32(0))
            OwnedBPME = txtBPME.Text
            txtBPTE.Text = CStr(readerBP.GetInt32(1))
            OwnedBPPE = txtBPTE.Text
            OwnedBP = True
            txtBPAddlCosts.Text = FormatNumber(readerBP.GetDouble(2), 2)
            OwnedBPRuns = readerBP.GetInt32(3)
        Else ' If sent from manufacturing tab, use the values set from there
            OwnedBP = False
            OwnedBPRuns = 1
            If TempTech = 1 Then ' All T1
                If SentFrom <> SentFromLocation.ManufacturingTab Then
                    txtBPME.Text = CStr(UserApplicationSettings.DefaultBPME)
                    txtBPTE.Text = CStr(UserApplicationSettings.DefaultBPTE)
                ElseIf SentFrom = SentFromLocation.ShoppingList Then
                    ' Will be set already or use default
                    If Trim(txtBPME.Text) = "" Then
                        txtBPME.Text = CStr(UserApplicationSettings.DefaultBPME)
                    End If
                    txtBPTE.Text = CStr(UserApplicationSettings.DefaultBPTE)
                End If
            Else ' Default T2/T3 BPCs are going to be copies
                If NewBP Then
                    txtBPME.Text = CStr(BaseT2T3ME + SelectedDecryptor.MEMod)
                    txtBPTE.Text = CStr(BaseT2T3TE + SelectedDecryptor.TEMod)
                End If
            End If
        End If

        Dim TempBPType As BPType

        If OwnedBP Then
            TempBPType = GetBPType(readerBP.GetInt32(4))
        Else
            TempBPType = BPType.NotOwned
        End If

        If TempTech <> 1 And TempBPType <> BPType.Original Then
            Call SetInventionEnabled("T" & CStr(TempTech), True) ' First enable then let the ignore invention check override if needed
            chkBPIgnoreInvention.Checked = UserBPTabSettings.IgnoreInvention

            ' disable the me/te boxes since these are invented
            If chkBPIgnoreInvention.Checked Then
                txtBPME.Enabled = True
                txtBPTE.Enabled = True
            Else
                txtBPME.Enabled = False
                txtBPTE.Enabled = False
            End If

        Else ' Check the ignore invention, they own this BPO and don't need to invent it (if T2)
            If TempTech = 2 Then
                chkBPIgnoreInvention.Checked = True
            End If
            ' enable the me/te boxes
            txtBPME.Enabled = True
            txtBPTE.Enabled = True
        End If

        If TempTech <> 2 Then
            chkBPIgnoreInvention.Enabled = False ' can't invent t1, and T3 are always invented - so don't allow toggle
        Else
            chkBPIgnoreInvention.Enabled = True ' All T2 options need the toggle
        End If

        ' Reactions can't have ME or TE
        If SelectedBPText.Contains("Reaction Formula") Then
            txtBPME.Enabled = False
            txtBPTE.Enabled = False
        End If

        gbBPManualSystemCostIndex.Enabled = True
        gbBPIgnoreinCalcs.Enabled = True
        btnBPUpdateCostIndex.Enabled = False

        cmbBPBlueprintSelection.Focus()

        ' Reset the combo for invention, and Load the relic types for BP selected for T3
        If NewBP Then
            Dim TempDName As String = ""
            If TempBPType = BPType.InventedBPC Or TempBPType = BPType.Copy Then
                ' Load the decryptor based on ME/TE
                Dim TempD As New DecryptorList
                LoadingInventionDecryptors = True
                LoadingT3Decryptors = True
                InventionDecryptorsLoaded = False
                T3DecryptorsLoaded = False
                ' Load up the decryptor based on data entered or BP data from an owned bp
                SelectedDecryptor = TempD.GetDecryptor(CInt(txtBPME.Text), CInt(txtBPTE.Text), OwnedBPRuns, TempTech)
                If SelectedDecryptor.Name = None And CInt(txtBPME.Text) <> BaseT2T3ME And CInt(txtBPTE.Text) <> BaseT2T3TE And TempBPType = BPType.Copy Then
                    TempDName = Unknown
                Else
                    TempDName = SelectedDecryptor.Name
                End If

                If TempTech = 2 Then
                    cmbBPInventionDecryptor.Text = TempDName
                ElseIf TempTech = 3 Then
                    cmbBPT3Decryptor.Text = TempDName
                End If

                LoadingInventionDecryptors = False
                LoadingT3Decryptors = False
            Else
                Call ResetDecryptorCombos(TempTech)
            End If

            If TempTech = 3 Then
                ' Load up the relic based on the bp data
                Call LoadRelicTypes(BPTypeID)
                Dim Tempstring As String
                Tempstring = GetRelicfromInputs(SelectedDecryptor, BPTypeID, OwnedBPRuns)
                If Tempstring <> "" Then
                    LoadingRelics = True
                    ' if found, set it else
                    cmbBPRelic.Text = Tempstring
                    LoadingRelics = False
                End If
            End If

        End If

        ' Make sure everything is enabled on first BP load
        If ResetBPTab Then
            btnBPRefreshBP.Enabled = True
            btnBPCopyMatstoClip.Enabled = True
            btnBPAddBPMatstoShoppingList.Enabled = True
            chkBPSimpleCopy.Enabled = True
            txtBPRuns.Enabled = True
            txtBPAddlCosts.Enabled = True
            chkBPBuildBuy.Enabled = True
            txtBPNumBPs.Enabled = True
            txtBPLines.Enabled = True
            chkBPTaxes.Enabled = True
            chkBPBrokerFees.Enabled = True
            chkBPPricePerUnit.Enabled = True

            btnBPBack.Enabled = True
            btnBPForward.Enabled = True

            ResetBPTab = False ' Reset
        End If

        readerBP.Close()
        readerBP = Nothing
        DBCommand = Nothing

        Application.DoEvents()

        ' Update the grid
        Call UpdateBPGrids(BPTypeID, TempTech, NewBP, ItemGroupID, ItemCategoryID, SentFromLocation.BlueprintTab)

        ' Save the blueprint in the history if it's not already in there
        If Not IsNothing(SelectedBlueprint) And SentFrom <> SentFromLocation.History Then
            Call UpdateBPHistory(True)
        End If

        txtBPRuns.SelectAll()
        txtBPRuns.Focus()
        cmbBPBlueprintSelection.SelectionLength = 0

    End Sub

    ' Updates the lists with the correct materials for the selected item
    Public Sub UpdateBPGrids(ByVal BPID As Integer, ByVal BPTech As Integer, ByVal NewBPSelection As Boolean,
                              BPGroupID As Integer, BPCategoryID As Integer, ByVal SentFrom As SentFromLocation)
        Dim IndustrySkill As Integer = 0
        Dim i As Integer = 0
        Dim BPRawMats As List(Of Material)
        Dim BPComponentMats As List(Of Material)
        Dim rawlstViewRow As ListViewItem
        Dim complstViewRow As ListViewItem
        Dim TempME As String = "0"
        Dim TempPrice As Double = 0
        Dim BPCName As String = ""

        ' For Invention Copy data - set defaults here
        Dim T1CopyRuns As Integer = 0
        Dim CopyCostPerSecond As Double = 0
        Dim SQL As String = ""
        Dim AdditionalCosts As Double

        Dim BPME As Integer = 0
        Dim BPTE As Integer = 0

        ' T2/T3 variables
        Dim RelicName As String = ""

        Dim SelectedRuns As Integer
        Dim ZeroCostToolTipText As String = ""

        ' Set the number of runs to 1 if it's blank
        If Trim(txtBPRuns.Text) = "" Then
            txtBPRuns.Text = "1"
        End If

        ' Check the quantity
        If Not IsNumeric(txtBPRuns.Text) Or Val(txtBPRuns.Text) <= 0 Then
            MsgBox("You must enter a valid number of runs", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPRuns.Focus()
            Exit Sub
        Else
            SelectedRuns = CInt(txtBPRuns.Text)
        End If

        ' Check the num bps
        If Not IsNumeric(txtBPNumBPs.Text) Or Trim(txtBPNumBPs.Text) = "" Then
            MsgBox("You must enter a valid number of BPs", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPNumBPs.Focus()
            Exit Sub
        End If

        ' Additional costs
        If Not IsNumeric(txtBPAddlCosts.Text) Or Trim(txtBPAddlCosts.Text) = "" Then
            MsgBox("You must enter a valid additional cost value", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPAddlCosts.Focus()
            Exit Sub
        Else
            ' Set the additional costs (this is just a raw value they enter)
            AdditionalCosts = CDbl(txtBPAddlCosts.Text)
        End If

        ' Check num lines
        If Not IsNumeric(txtBPLines.Text) Or Val(txtBPLines.Text) <= 0 Then
            MsgBox("You must enter a valid number of Production Lines", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPLines.Focus()
            Exit Sub
        End If

        ' Check the laboratory lines
        If Not IsNumeric(txtBPInventionLines.Text) Or Val(txtBPInventionLines.Text) <= 0 Then
            MsgBox("You must enter a valid number of Invention Lines", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPInventionLines.Focus()
            Exit Sub
        End If

        If Not IsNumeric(txtBPRelicLines.Text) Or Val(txtBPRelicLines.Text) <= 0 Then
            MsgBox("You must enter a valid number of T3 Invention Lines", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPRelicLines.Focus()
            Exit Sub
        End If

        ' Check num bps
        If Not IsNumeric(txtBPNumBPs.Text) Or Val(txtBPNumBPs.Text) <= 0 Then
            MsgBox("You must enter a valid number of BPs", vbExclamation, Application.ProductName)
            txtBPRuns.SelectAll()
            txtBPRuns.Focus()
            Exit Sub
        End If

        ' Facility setup
        Dim ManufacturingFacility As IndustryFacility = BPTabFacility.GetSelectedManufacturingFacility ' This is the facility to manufacture the item in the blueprint
        Dim ComponentManufacturingFacility As IndustryFacility = BPTabFacility.GetFacility(ProductionType.ComponentManufacturing)
        Dim CapitalComponentManufacturingFacility As IndustryFacility = BPTabFacility.GetFacility(ProductionType.CapitalComponentManufacturing)
        Dim CopyFacility As IndustryFacility = BPTabFacility.GetFacility(ProductionType.Copying)
        Dim InventionFacility As New IndustryFacility

        If BPTech = BPTechLevel.T2 Or BPTech = BPTechLevel.T3 Then
            ' Set the invention facility data
            If BPTech = BPTechLevel.T3 Then
                ' Need to add the relic variant to the query for just one item
                RelicName = cmbBPRelic.Text
                InventionFacility = BPTabFacility.GetFacility(ProductionType.T3Invention)
                ' Load the decryptor options
                Call LoadBPT3InventionDecryptors()
            ElseIf BPTech = BPTechLevel.T2 Then
                ' T2 no relic 
                RelicName = ""
                InventionFacility = BPTabFacility.GetFacility(ProductionType.Invention)
                ' Load the decryptor options
                Call LoadBPInventionDecryptors()
                ' T2 has copy costs/time
                CopyFacility.IncludeActivityCost = chkBPIncludeCopyCosts.Checked
                CopyFacility.IncludeActivityTime = chkBPIncludeCopyTime.Checked
            End If

            ' All invention facilities need to have these set
            InventionFacility.IncludeActivityCost = chkBPIncludeInventionCosts.Checked
            InventionFacility.IncludeActivityTime = chkBPIncludeInventionTime.Checked
        End If

        ' Working

        ' Now load the materials into the lists
        lstBPComponentMats.Items.Clear()
        lstBPComponentMats.Enabled = False
        lstBPRawMats.Items.Clear()
        lstBPRawMats.Enabled = False
        lblBPCanMakeBP.Visible = False
        lblBPCanMakeBPAll.Visible = False
        txtListEdit.Visible = False
        btnBPRefreshBP.Enabled = False
        Me.Cursor = Cursors.WaitCursor
        IgnoreFocus = True
        Application.DoEvents()
        IgnoreFocus = False

        BPME = CInt(txtBPME.Text)
        BPTE = CInt(txtBPTE.Text)

        ' Construct our Blueprint
        SelectedBlueprint = New Blueprint(BPID, SelectedRuns, BPME, BPTE, CInt(txtBPNumBPs.Text), CInt(txtBPLines.Text), SelectedCharacter,
                                          UserApplicationSettings, chkBPBuildBuy.Checked, AdditionalCosts, ManufacturingFacility,
                                          ComponentManufacturingFacility, CapitalComponentManufacturingFacility)

        ' Set the T2 and T3 inputs if necessary
        If BPTech <> BPTechLevel.T1 And chkBPIgnoreInvention.Checked = False Then
            ' invent this bp
            txtBPNumBPs.Text = CStr(SelectedBlueprint.InventBlueprint(CInt(txtBPInventionLines.Text), SelectedDecryptor,
                                  InventionFacility, CopyFacility, GetInventItemTypeID(BPID, RelicName)))
        End If

        ' Build the item and get the list of materials
        Call SelectedBlueprint.BuildItems(chkBPTaxes.Checked, chkBPBrokerFees.Checked, False, chkBPIgnoreMinerals.Checked, chkBPIgnoreT1Item.Checked)

        ' Get the lists
        BPRawMats = SelectedBlueprint.GetRawMaterials.GetMaterialList
        BPComponentMats = SelectedBlueprint.GetComponentMaterials.GetMaterialList

        If chkBPBuildBuy.Checked Then
            lblBPComponentMats.Text = "Build/Buy Component Material List"
            lblBPRawMats.Text = "Build/Buy Raw Material List"
            lblBPBuildColor.Visible = True
            lblBPBuyColor.Visible = True
        Else ' Show all
            lblBPComponentMats.Text = "Component Material List"
            lblBPRawMats.Text = "Raw Material List"
            lblBPBuildColor.Visible = False
            lblBPBuyColor.Visible = False
        End If

        ' Fill Component List if components built
        If Not IsNothing(BPComponentMats) And SelectedBlueprint.HasComponents Then
            lstBPComponentMats.Items.Clear()
            lstBPComponentMats.BeginUpdate()
            For i = 0 To BPComponentMats.Count - 1
                'complstViewRow = lstBPComponentMats.Items.Add(BPComponentMats(i).GetMaterialName) ' Check TODO - Add check box?
                'The remaining columns are subitems  
                complstViewRow = New ListViewItem(BPComponentMats(i).GetMaterialName)
                complstViewRow.SubItems.Add(FormatNumber(BPComponentMats(i).GetQuantity, 0))
                TempME = BPComponentMats(i).GetItemME

                ' Mark line yellow if the blueprint for this item has no ME stored
                If TempME = "0" Then
                    complstViewRow.BackColor = Color.LightGray
                Else
                    complstViewRow.BackColor = Color.White
                End If

                ' If we want to build the item, then override the back color
                If chkBPBuildBuy.Checked Then
                    If BPComponentMats(i).GetBuildItem Then
                        complstViewRow.BackColor = lblBPBuildColor.BackColor
                    Else
                        complstViewRow.BackColor = lblBPBuyColor.BackColor
                    End If
                End If

                complstViewRow.SubItems.Add(TempME)
                TempPrice = BPComponentMats(i).GetCostPerItem

                ' If the price is zero, highlight text as red
                If TempPrice = 0 Then
                    complstViewRow.ForeColor = Color.Red
                Else
                    complstViewRow.ForeColor = Color.Black
                End If
                complstViewRow.SubItems.Add(FormatNumber(TempPrice, 2))
                complstViewRow.SubItems.Add(FormatNumber(BPComponentMats(i).GetTotalCost, 2))
                Call lstBPComponentMats.Items.Add(complstViewRow)
            Next

            Dim TempSort As SortOrder
            If BPCompColumnSortType = SortOrder.Ascending Then
                TempSort = SortOrder.Descending
            Else
                TempSort = SortOrder.Ascending
            End If

            ' Sort the component list
            Call ListViewColumnSorter(BPCompColumnClicked, CType(lstBPComponentMats, ListView), BPCompColumnClicked, TempSort)

            lstBPComponentMats.EndUpdate()

            ' Enable the raw and component selector radio for exporting to shopping list (only if we don't have calc build/buy checked)
            If chkBPBuildBuy.Checked = True Then
                rbtnBPRawmatCopy.Enabled = False
                rbtnBPComponentCopy.Enabled = True
            Else
                rbtnBPRawmatCopy.Enabled = True
                rbtnBPComponentCopy.Enabled = True
            End If

            If SentFrom <> SentFromLocation.ManufacturingTab Then
                Select Case UserBPTabSettings.ExporttoShoppingListType
                    Case rbtnBPComponentCopy.Text
                        rbtnBPComponentCopy.Checked = True
                    Case rbtnBPCopyInvREMats.Text
                        rbtnBPCopyInvREMats.Checked = True
                    Case rbtnBPRawmatCopy.Text
                        ' If the raw button isn't enabled, then default to components
                        If rbtnBPRawmatCopy.Enabled Then
                            rbtnBPRawmatCopy.Checked = True
                        Else
                            rbtnBPComponentCopy.Checked = True
                        End If
                End Select
                lstBPComponentMats.Enabled = True
            End If

        Else ' No components
            ' Disable the raw and component selector radio for exporting to shopping list, the button will still just pull the data from the list anyway though
            rbtnBPComponentCopy.Enabled = False
            rbtnBPRawmatCopy.Enabled = True
            rbtnBPRawmatCopy.Checked = True
            lstBPComponentMats.Enabled = False
        End If

        If SelectedBlueprint.GetTechLevel <> BPTechLevel.T1 Then
            ' Enable the invention mats
            rbtnBPCopyInvREMats.Enabled = True

            ' Set this value if it just got enabled and they want it
            If UserBPTabSettings.ExporttoShoppingListType = rbtnBPCopyInvREMats.Text Then
                rbtnBPCopyInvREMats.Checked = True
            End If
        Else
            rbtnBPCopyInvREMats.Enabled = False
        End If

        If Not IsNothing(BPRawMats) Then
            ' Fill the Raw List
            lstBPRawMats.Items.Clear()
            lstBPRawMats.BeginUpdate()
            If (chkBPCompressedOre.Checked) Then
                Call CalculateCompressedOres(BPRawMats)
            Else
                For i = 0 To BPRawMats.Count - 1
                    rawlstViewRow = New ListViewItem(BPRawMats(i).GetMaterialName)
                    'The remaining columns are subitems  
                    rawlstViewRow.SubItems.Add(FormatNumber(BPRawMats(i).GetQuantity, 0))
                    rawlstViewRow.SubItems.Add(BPRawMats(i).GetItemME)
                    TempPrice = BPRawMats(i).GetCostPerItem
                    ' If the price is zero, highlight text as red
                    If TempPrice = 0 Then
                        rawlstViewRow.ForeColor = Color.Red
                    Else
                        rawlstViewRow.ForeColor = Color.Black
                    End If
                    rawlstViewRow.SubItems.Add(FormatNumber(TempPrice, 2))
                    rawlstViewRow.SubItems.Add(FormatNumber(BPRawMats(i).GetTotalCost, 2))
                    Call lstBPRawMats.Items.Add(rawlstViewRow)
                Next
            End If
            ' Sort the raw mats list
            Dim TempSort As SortOrder
            If BPRawColumnSortType = SortOrder.Ascending Then
                TempSort = SortOrder.Descending
            Else
                TempSort = SortOrder.Ascending
            End If

            Call ListViewColumnSorter(BPRawColumnClicked, CType(lstBPRawMats, ListView), BPRawColumnClicked, TempSort)

            lstBPRawMats.EndUpdate()
        End If

        ' Get the production time
        If chkBPBuildBuy.Checked Then
            ' Grey this out because it doesn't really apply here
            lblBPProductionTime.Enabled = False
        Else
            lblBPProductionTime.Enabled = True
        End If

        ' Reset the number of bps to what we used in batches, not what was entered
        txtBPNumBPs.Text = CStr(SelectedBlueprint.GetUsedNumBPs)

        ' Show and update labels for T2 if selected
        If SelectedBlueprint.GetTechLevel = BPTechLevel.T2 Then
            If chkBPIgnoreInvention.Checked = False Then
                If SelectedBlueprint.UserCanInventRE Then
                    lblBPT2InventStatus.Text = "Invention Calculations:"
                    lblBPT2InventStatus.ForeColor = Color.Black
                Else
                    lblBPT2InventStatus.Text = "Cannot Invent - Typical Cost Shown"
                    lblBPT2InventStatus.ForeColor = Color.Red
                End If

                ' Invention cost to get enough success for the runs entered
                lblBPInventionCost.Text = FormatNumber(SelectedBlueprint.GetInventionCost(), 2)

                ' Add copy costs for enough succesful runs
                lblBPCopyCosts.Text = FormatNumber(SelectedBlueprint.GetCopyCost, 2)

                ' Invention Chance
                lblBPInventionChance.Text = FormatPercent(SelectedBlueprint.GetInventionChance(), 2)

                ' Update the decryptor stats box ME: -4, TE: -3, Runs: +9
                lblBPDecryptorStats.Text = "ME: " & CStr(SelectedDecryptor.MEMod) & ", TE: " & CStr(SelectedDecryptor.TEMod) & vbCrLf & "BP Runs: " & CStr(SelectedBlueprint.GetSingleInventedBPCRuns)

                ' Show the copy time if they want it
                lblBPCopyTime.Text = FormatIPHTime(SelectedBlueprint.GetCopyTime)

                ' Show the invention time if they want it
                lblBPInventionTime.Text = FormatIPHTime(SelectedBlueprint.GetInventionTime)

                ' Set the tool tip for copy costs to the invention chance label
                ttBP.SetToolTip(lblBPInventionChance, SelectedBlueprint.GetInventionBPC)

                ' Finally check the invention materials and make sure that if any have 0.00 for price,
                ' we update the invention label and add a tooltip for what has a price of 0
                If Not IsNothing(SelectedBlueprint.GetInventionMaterials.GetMaterialList) Then
                    With SelectedBlueprint.GetInventionMaterials
                        For i = 0 To .GetMaterialList.Count - 1
                            If .GetMaterialList(i).GetTotalCost = 0 And Not (.GetMaterialList(i).GetMaterialName.Contains("Blueprint") Or .GetMaterialList(i).GetMaterialName.Contains("Data Interface")) Then
                                ZeroCostToolTipText = ZeroCostToolTipText & .GetMaterialList(i).GetMaterialName & ", "
                            End If
                        Next
                    End With
                End If

                If ZeroCostToolTipText <> "" Then
                    ' We have a few zero priced items
                    ZeroCostToolTipText = ZeroCostToolTipText.Substring(0, Len(ZeroCostToolTipText) - 2)
                    ZeroCostToolTipText = "Invention Costs may be inaccurate; the following items have 0.00 for price: " & ZeroCostToolTipText
                    lblBPT2InventStatus.ForeColor = Color.Red
                    ttBP.SetToolTip(lblBPT2InventStatus, ZeroCostToolTipText)
                Else
                    lblBPT2InventStatus.ForeColor = Color.Black
                    ttBP.SetToolTip(lblBPT2InventStatus, "")
                End If
            Else
                FirstLoad = True
                Call ResetInventionBoxes()
                FirstLoad = False
            End If

            ' Show the invention tabs
            tabBPInventionEquip.TabPages.Remove(tabT3Calcs)
            If Not tabBPInventionEquip.TabPages.Contains(tabInventionCalcs) Then
                tabBPInventionEquip.TabPages.Add(tabInventionCalcs)
            End If

            ' Enable option
            rbtnBPCopyInvREMats.Enabled = True

        ElseIf SelectedBlueprint.GetTechLevel = BPTechLevel.T3 Then
            ' Show the RE calc tab
            tabBPInventionEquip.TabPages.Remove(tabInventionCalcs)
            If Not tabBPInventionEquip.TabPages.Contains(tabT3Calcs) Then
                tabBPInventionEquip.TabPages.Add(tabT3Calcs)
            End If

            If chkBPIgnoreInvention.Checked = False Then
                ' RE Cost and time
                lblBPRECost.Text = FormatNumber(SelectedBlueprint.GetInventionCost(), 2)
                lblBPRETime.Text = FormatIPHTime(SelectedBlueprint.GetInventionTime())

                ' Update the decryptor stats box ME: -4, TE: -3, Runs: +9
                lblBPT3Stats.Text = "ME: " & CStr(SelectedDecryptor.MEMod) & ", TE: " & CStr(SelectedDecryptor.TEMod) & "," & vbCrLf & "BP Runs: " & CStr(SelectedBlueprint.GetSingleInventedBPCRuns)

                If SelectedBlueprint.UserCanInventRE Then
                    lblT3InventStatus.Text = "T3 Invention Calculations:"
                    lblT3InventStatus.ForeColor = Color.Black
                Else
                    lblT3InventStatus.Text = "Cannot Invent - Typical Cost Shown"
                    lblT3InventStatus.ForeColor = Color.Red
                End If

                lblBPT3InventionChance.Text = FormatPercent(SelectedBlueprint.GetInventionChance(), 2)

                ' Enable option for adding mats to shopping list
                rbtnBPCopyInvREMats.Enabled = True

                ' Finally check the RE materials and make sure that if any have 0.00 for price,
                ' we update the RE label and add a tooltip for what has a price of 0
                If Not IsNothing(SelectedBlueprint.GetInventionMaterials.GetMaterialList) Then
                    With SelectedBlueprint.GetInventionMaterials
                        For i = 0 To .GetMaterialList.Count - 1
                            If .GetMaterialList(i).GetTotalCost = 0 Then
                                ZeroCostToolTipText = ZeroCostToolTipText & .GetMaterialList(i).GetMaterialName & ", "
                            End If
                        Next
                    End With
                End If

                If ZeroCostToolTipText <> "" Then
                    ' We have a few zero priced items
                    ZeroCostToolTipText = ZeroCostToolTipText.Substring(0, Len(ZeroCostToolTipText) - 2)
                    ZeroCostToolTipText = "T3 Invention Costs may be inaccurate; the following items have 0.00 for price: " & ZeroCostToolTipText
                    lblT3InventStatus.ForeColor = Color.Red
                    ttBP.SetToolTip(lblT3InventStatus, ZeroCostToolTipText)
                Else
                    lblBPT2InventStatus.ForeColor = Color.Black
                    ttBP.SetToolTip(lblT3InventStatus, "")
                End If
            Else
                FirstLoad = True
                Call ResetInventionBoxes()
                FirstLoad = False
            End If

        Else ' T1
            If rbtnBPCopyInvREMats.Checked Then
                ' We are turning this off, so move to raw
                rbtnBPRawmatCopy.Checked = True
            End If
            rbtnBPCopyInvREMats.Enabled = False

            ' Remove calcs for t1
            tabBPInventionEquip.TabPages.Remove(tabInventionCalcs)
            tabBPInventionEquip.TabPages.Remove(tabT3Calcs)
        End If

        ' Set the tab to the one selected
        If SelectedBPTabIndex <= tabBPInventionEquip.TabCount - 1 Then
            tabBPInventionEquip.SelectTab(SelectedBPTabIndex)
        Else
            tabBPInventionEquip.SelectTab(0)
        End If

        ' Finally Update the labels
        Call UpdateBPPriceLabels()

ExitForm:

        ' If the bp was updated (not new, then save any changes to the history - e.g. facility changes)
        If Not NewBPSelection And SentFrom = SentFromLocation.BlueprintTab Then
            Call UpdateBPHistory(False)
        End If

        LoadingBPfromHistory = False

        ' Done
        lstBPComponentMats.Enabled = True
        lstBPRawMats.Enabled = True
        lblBPCanMakeBP.Visible = True
        lblBPCanMakeBPAll.Visible = True
        btnBPRefreshBP.Enabled = True

        ' Enable facility selectors
        tabBPInventionEquip.Enabled = True

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub CalculateCompressedOres(ByVal bpMaterialList As List(Of Material))
        Dim newList As New List(Of OreMineral)
        Dim oreQuantityList As ListViewItem
        Dim materialQuantityList As ListViewItem
        Dim materialList As New List(Of Material)
        Dim oreID As Integer
        Dim oreSkillReproSkillID As Integer
        Dim reproSkill As Integer
        Dim reproEffSkill As Integer
        Dim reproSpecOreSkill As Integer
        Dim refinePercent As Double ' Start with 50% refining
        Dim stationTax As Double
        Dim lockedList As New List(Of Integer)
        Dim mineralTotal As Double
        Dim oreCost As Double

        Dim skillDict As New Dictionary(Of String, Integer) From {
                {"Arkonor", 12180},
                {"Bistot", 12181},
                {"Crokite", 12182},
                {"Dark Ochre", 12183},
                {"Gneiss", 12184},
                {"Hedbergite", 12185},
                {"Hemorphite", 12186},
                {"Jaspet", 12187},
                {"Kernite", 12188},
                {"Mercoxit", 12189},
                {"Omber", 12190},
                {"Plagioclase", 12191},
                {"Pyroxeres", 12192},
                {"Scordite", 12193},
                {"Spodumain", 12194},
                {"Veldspar", 12195}}


        Dim oreSQL = "SELECT o.OreID, o.MineralID, o.MineralQuantity, i.typeName, g.groupName FROM ORE_REFINE o " +
                     "JOIN INVENTORY_TYPES i ON o.OreID = i.typeID " +
                     "JOIN INVENTORY_GROUPS g ON i.groupID = g.groupID " +
                     "WHERE o.OreID = {0} " +
                     "ORDER BY o.MineralQuantity DESC"

        Dim mineralSQL = "SELECT o.OreID FROM ORE_REFINE o " +
                         "JOIN INVENTORY_TYPES i ON o.OreID = i.typeID " +
                         "WHERE i.typeName LIKE 'Compressed%' " +
                         "AND o.MineralID = {0} " +
                         "ORDER BY o.MineralQuantity DESC LIMIT 1"

        reproSkill = SelectedCharacter.Skills.GetSkillLevel(3385)
        reproEffSkill = SelectedCharacter.Skills.GetSkillLevel(3389)

        If BPTabFacility.GetFacility(BPTabFacility.GetCurrentFacilityProductionType).GetFacilityTypeDescription = ManufacturingFacility.StationFacility Then
            Using DBCommand = New SQLiteCommand(String.Format("SELECT REPROCESSING_EFFICIENCY FROM STATIONS WHERE STATION_NAME = '{0}'", BPTabFacility.GetFacility(BPTabFacility.GetCurrentFacilityProductionType).FacilityName), EVEDB.DBREf)
                refinePercent = CType(DBCommand.ExecuteScalar, Double)
            End Using
        ElseIf BPTabFacility.GetFacility(BPTabFacility.GetCurrentFacilityProductionType).GetFacilityTypeDescription = ManufacturingFacility.OutpostFacility Or
                BPTabFacility.GetFacility(BPTabFacility.GetCurrentFacilityProductionType).GetFacilityTypeDescription = ManufacturingFacility.StructureFacility Then
            stationTax = 0.0
            refinePercent = 0.5
        ElseIf BPTabFacility.GetFacility(BPTabFacility.GetCurrentFacilityProductionType).GetFacilityTypeDescription = ManufacturingFacility.POSFacility Then
            stationTax = 0.0
            refinePercent = 0.52
        End If

        If refinePercent = 0 Then
            refinePercent = 0.5 ' Setting the refine percent to 50 if it comes back as 0 for corrupt station data.
        End If

        For i = 0 To bpMaterialList.Count - 1 Step 1
            Dim loopCounter = i
            Dim currentMineralID = CType(bpMaterialList(loopCounter).GetMaterialTypeID(), Integer)

            If (currentMineralID > 40) Then
                materialList.Add(bpMaterialList(i))
                Continue For
            End If

            Using DBCommand = New SQLiteCommand(String.Format(mineralSQL, currentMineralID), EVEDB.DBREf)
                oreID = CType(DBCommand.ExecuteScalar(), Integer)
            End Using

            If oreID = 28367 And currentMineralID = 36 Then
                oreID = 28397
            End If

            ' Reprocessing = 3385 -> 0.03 => 0.15
            ' Repro Efficiency = 3389 -> 0.02 => 0.10
            ' Ore Special = 12180-12195 -> 0.02 => 0.10
            ' Station Equipment x (1 + Processing skill x 0.03) x (1 + Processing Efficiency skill x 0.02) x (1 + Ore Processing skill x 0.02) x (1 + Processing Implant)
            ' Beancounter 27169, 27174, 27175 (2, 4, 1) 

            Using DBCommand = New SQLiteCommand(String.Format(oreSQL, oreID), EVEDB.DBREf)
                Dim result = DBCommand.ExecuteReader()
                While result.Read()
                    skillDict.TryGetValue(result.GetString(4), oreSkillReproSkillID)
                    reproSpecOreSkill = SelectedCharacter.Skills.GetSkillLevel(oreSkillReproSkillID)

                    Dim mineralRefinePercent As Double = refinePercent * (1 + reproSkill * 0.03) * (1 + reproEffSkill * 0.02) * (1 + reproSpecOreSkill * 0.02) * (1 + UserApplicationSettings.RefiningImplantValue)

                    Dim mineralQuantity = result.GetInt32(2) * mineralRefinePercent

                    'Dim mineralList = newList.Where(Function(b) b.MineralID = currentMineralID)



                    ' TODO : FIX THE MULTIPLIER
                    mineralTotal = newList.Where(Function(b) b.MineralID = currentMineralID).Sum(Function(a) a.MineralQuantity * a.OreMultiplier)

                    If mineralTotal < bpMaterialList(loopCounter).GetQuantity() Or currentMineralID <> result.GetInt32(1) Then
                        newList.Add(New OreMineral With {
                                    .OreID = result.GetInt32(0),
                                    .MineralID = result.GetInt32(1),
                                    .MineralQuantity = mineralQuantity,
                                    .OreMultiplier = 0,
                                    .OreName = result.GetString(3),
                                    .OreSelectedFor = currentMineralID})
                    End If
                End While

                ' Make sure we're not grabbing the same Ore numerous times.
                Dim currentMineral = newList.FirstOrDefault(Function(x) x.OreID = oreID And x.MineralID = currentMineralID And x.Locked = False)

                If currentMineral Is Nothing Then
                    Continue For
                End If

                If currentMineralID = 35 And oreID = 28420 Then
                    mineralTotal = 0
                End If
                Dim multiplier = (bpMaterialList(loopCounter).GetQuantity() - mineralTotal) / currentMineral.MineralQuantity

                If (multiplier > 0) Then
                    Dim updateMultipliers = newList.Where(Function(y) y.OreID = currentMineral.OreID And y.OreSelectedFor = currentMineralID)
                    lockedList.Add(oreID)
                    For Each item As OreMineral In updateMultipliers
                        item.OreMultiplier = CType(Math.Ceiling(multiplier), Int64)
                        ' If an ore has been 'multiplied' then lock it so we can no longer modify it.
                        item.Locked = True
                    Next
                End If

                ' Moved this down here because the Tritanium/Pyerite problem wasn't getting resolved if the item only required the two minerals.
                ' This will fix the issue by forcing it to reset Spodumain properly.
                Dim mineralList = newList.Where(Function(b) b.MineralID = currentMineralID)

                Dim tempList = mineralList.OrderByDescending(Function(c) c.OreMultiplier).Where(Function(o) o.OreID = 28420)

                If tempList.Count > 1 Then
                    Dim tmpRange = newList.Where(Function(y) y.OreID = 28420 And y.OreSelectedFor = tempList(1).OreSelectedFor And y.OreMultiplier > 0)
                    For Each item As OreMineral In tmpRange
                        item.OreMultiplier = 0
                    Next
                End If

            End Using

        Next

        'Populate the final list with distinct ore names (no point showing Compressed Arkonor 3 times for each mineral type)
        Dim oreList = newList.Where(Function(x) x.OreMultiplier > 0).DistinctBy(Function(c) c.OreSelectedFor)

        For Each item As OreMineral In oreList
            oreQuantityList = New ListViewItem(item.OreName)
            oreQuantityList.SubItems.Add(CType(item.OreMultiplier, String))
            oreQuantityList.SubItems.Add("-")
            Using DBCommand = New SQLiteCommand(String.Format("SELECT AVERAGE_PRICE FROM ITEM_PRICES WHERE ITEM_ID = {0}", item.OreID), EVEDB.DBREf)
                Dim avgPrice = CType(DBCommand.ExecuteScalar(), Double)
                oreQuantityList.SubItems.Add(FormatNumber(avgPrice, 2))
                oreQuantityList.SubItems.Add(FormatNumber(avgPrice * item.OreMultiplier, 2))

                oreCost += avgPrice * item.OreMultiplier
            End Using
            Call lstBPRawMats.Items.Add(oreQuantityList)

        Next

        For Each item As Material In materialList
            materialQuantityList = New ListViewItem(item.GetMaterialName())
            materialQuantityList.SubItems.Add(CType(item.GetQuantity(), String))
            materialQuantityList.SubItems.Add("-")
            materialQuantityList.SubItems.Add(FormatNumber(item.GetCostPerItem(), 2))
            materialQuantityList.SubItems.Add(FormatNumber(item.GetTotalCost(), 2))
            oreCost += item.GetTotalCost()

            Call lstBPRawMats.Items.Add(materialQuantityList)
        Next

        lblBPRawMatCost.Text = FormatNumber(oreCost, 2)


    End Sub

    ' Updates the blueprint history list for moving forward and back
    Private Sub UpdateBPHistory(ByVal NewBP As Boolean)
        Dim TempBPHistoryItem As BPHistoryItem
        Dim BP As Blueprint = SelectedBlueprint

        If Not IsNothing(BP) Then
            If Not LoadingBPfromHistory Then
                With TempBPHistoryItem
                    .BPID = BP.GetTypeID
                    .BPName = BP.GetName
                    If chkBPBuildBuy.Checked Then
                        .BuildType = "Build/Buy"
                    Else
                        .BuildType = ""
                    End If

                    If BP.GetTechLevel = BPTechLevel.T2 Then
                        .Inputs = BP.GetDecryptor.Name
                    ElseIf BP.GetTechLevel = BPTechLevel.T3 Then
                        .Inputs = BP.GetDecryptor.Name & " - " & BP.GetRelic ' parse decryptor and relic
                    Else
                        .Inputs = ""
                    End If
                    .SentFrom = SentFromLocation.History
                    .BuildFacility = CType(BPTabFacility.GetSelectedManufacturingFacility(BP.GetItemGroupID, BP.GetItemCategoryID).Clone, IndustryFacility)
                    .ComponentFacility = CType(BPTabFacility.GetFacility(ProductionType.ComponentManufacturing).Clone, IndustryFacility)
                    .CapComponentFacility = CType(BPTabFacility.GetFacility(ProductionType.CapitalComponentManufacturing).Clone, IndustryFacility)
                    .CopyFacility = CType(BPTabFacility.GetFacility(ProductionType.Copying).Clone, IndustryFacility)
                    .InventionFacility = CType(BPTabFacility.GetSelectedInventionFacility(BP.GetItemGroupID, BP.GetItemCategoryID).Clone, IndustryFacility)
                    .SentRuns = txtBPRuns.Text
                    .IncludeTaxes = chkBPTaxes.Checked
                    .IncludeFees = chkBPBrokerFees.Checked
                    .MEValue = txtBPME.Text
                    .TEValue = txtBPTE.Text
                    .SentRuns = txtBPRuns.Text
                    .ManufacturingLines = txtBPLines.Text
                    If BP.GetTechLevel = BPTechLevel.T2 Then
                        .LabLines = txtBPInventionLines.Text
                    ElseIf BP.GetTechLevel = BPTechLevel.T3 Then
                        .LabLines = txtBPRelicLines.Text
                    Else
                        .LabLines = "1"
                    End If
                    .NumBPs = txtBPNumBPs.Text
                    .AddlCosts = txtBPAddlCosts.Text
                    .PPU = chkBPPricePerUnit.Checked
                End With

                ' Find where the last bp was and insert the new one - so if we have 10 bps, and then select a new bp (component) the component is now the last bp
                If NewBP Then
                    If CurrentBPHistoryIndex < 0 Then
                        Call BPHistory.Add(TempBPHistoryItem)
                        CurrentBPHistoryIndex = 0
                    Else
                        Call BPHistory.Insert(CurrentBPHistoryIndex + 1, TempBPHistoryItem)
                        CurrentBPHistoryIndex = CurrentBPHistoryIndex + 1
                    End If
                Else
                    ' Remove the old one and replace it with the one we have now
                    Call BPHistory.RemoveAt(CurrentBPHistoryIndex)
                    Call BPHistory.Insert(CurrentBPHistoryIndex, TempBPHistoryItem)
                End If

            End If
        End If

        Call UpdateBlueprintHistoryButtons()

    End Sub

    ' Selects the images to be shown in the picture when a blueprint is selected
    Private Sub LoadBlueprintPicture(ByVal BPID As Long, ByVal ItemType As Integer)
        Dim BPImage As String
        Dim BPTechImagePath As String = ""

        ' Load the image - use absolute value since I use negative bpid's for special bps
        BPImage = Path.Combine(UserImagePath, CStr(Math.Abs(BPID)) & "_64.png")

        ' Check for the Tech Image
        If File.Exists(BPImage) Then
            pictBP.Image = Image.FromFile(BPImage)
        Else
            pictBP.Image = Nothing
        End If

        pictBP.Update()

    End Sub

    ' Selects and sets the decryptor
    Private Function SelectDecryptor(ByVal DecryptorText As String) As Decryptor

        If DecryptorText = None Or DecryptorText = "" Then
            SelectedDecryptor = NoDecryptor
        Else
            Dim InventionDecryptors As New DecryptorList()
            SelectedDecryptor = InventionDecryptors.GetDecryptor(DecryptorText)
        End If

        ' Set the ME/TE text here
        txtBPME.Text = CStr(SelectedDecryptor.MEMod + BaseT2T3ME)
        txtBPTE.Text = CStr(SelectedDecryptor.TEMod + BaseT2T3TE)

        Return SelectedDecryptor

    End Function

    ' Updates the price and other labels on the BP tab for the selected BP
    Public Sub UpdateBPPriceLabels()
        ' For final printout in boxes
        Dim TotalRawProfit As Double
        Dim TotalCompProfit As Double
        Dim TotalRawIPH As Double
        Dim TotalCompIPH As Double
        Dim DivideUnits As Long

        If chkBPPricePerUnit.Checked Then
            ' Need to divide all values by the total units produced
            ' This will only update the values in the top right box
            DivideUnits = SelectedBlueprint.GetTotalUnits
            ' Show only 1 unit in the units label
            lblBPTotalUnits.Text = "1"
        Else
            ' Just keep everything the same
            DivideUnits = 1
            ' Show the total units
            lblBPTotalUnits.Text = FormatNumber(SelectedBlueprint.GetTotalUnits, 0)
        End If

        ' Find the market price for the produced item
        lblBPMarketCost.Text = FormatNumber(SelectedBlueprint.GetItemMarketPrice / DivideUnits, 2)

        ' Materials (bottom labels)
        If Not chkBPCompressedOre.Checked Then
            lblBPRawMatCost.Text = FormatNumber(SelectedBlueprint.GetRawMaterials.GetTotalMaterialsCost, 2)
        End If

        lblBPComponentMatCost.Text = FormatNumber(SelectedBlueprint.GetComponentMaterials.GetTotalMaterialsCost, 2)

        ' Taxes/Fees
        lblBPTaxes.Text = FormatNumber(SelectedBlueprint.GetSalesTaxes / DivideUnits, 2)
        lblBPBrokerFees.Text = FormatNumber(SelectedBlueprint.GetSalesBrokerFees / DivideUnits, 2)

        ' Update usage labels
        Call UpdateFacilityUsage(DivideUnits)

        ' Total
        lblBPRawTotalCost.Text = FormatNumber((SelectedBlueprint.GetTotalRawCost) / DivideUnits, 2)
        lblBPTotalCompCost.Text = FormatNumber((SelectedBlueprint.GetTotalComponentCost) / DivideUnits, 2)

        ' Profit labels (market cost - total cost of mats and invention)
        TotalRawProfit = SelectedBlueprint.GetTotalRawProfit / DivideUnits

        If TotalRawProfit < 0 Then
            lblBPRawProfit.ForeColor = Color.Red
        Else
            lblBPRawProfit.ForeColor = Color.Black
        End If

        TotalCompProfit = SelectedBlueprint.GetTotalComponentProfit / DivideUnits

        If TotalCompProfit < 0 Then
            lblBPCompProfit.ForeColor = Color.Red
        Else
            lblBPCompProfit.ForeColor = Color.Black
        End If

        ' Profit labels, check what type
        If lblBPRawProfit1.Text.Contains("Percent") Then
            lblBPRawProfit.Text = FormatPercent(SelectedBlueprint.GetTotalRawProfitPercent, 2)
        Else
            lblBPRawProfit.Text = FormatNumber(TotalRawProfit, 2)
        End If

        If lblBPCompProfit1.Text.Contains("Percent") Then
            lblBPCompProfit.Text = FormatPercent(SelectedBlueprint.GetTotalComponentProfitPercent, 2)
        Else
            lblBPCompProfit.Text = FormatNumber(TotalCompProfit, 2)
        End If

        If DivideUnits = 1 Then
            TotalRawIPH = SelectedBlueprint.GetTotalIskperHourRaw
            TotalCompIPH = SelectedBlueprint.GetTotalIskperHourComponents
        Else ' Need to adjust the production time per unit then calck IPH
            ' ISK per Hour (divide total cost by production time in seconds for a isk per second calc, then multiply by 3600 for isk per hour)
            TotalRawIPH = TotalRawProfit / (SelectedBlueprint.GetTotalProductionTime / DivideUnits) * 3600 ' Build everything

            ' If we are doing build/buy then the total IPH will be the same as RAW since the lists are identical for what to buy 
            If chkBPBuildBuy.Checked Then
                TotalCompIPH = TotalRawIPH
            Else
                TotalCompIPH = TotalCompProfit / (SelectedBlueprint.GetProductionTime / DivideUnits) * 3600 ' Buy all components, just production time of BP
            End If

        End If

        If TotalRawProfit < 0 Then
            lblBPRawIPH.ForeColor = Color.Red
        Else
            lblBPRawIPH.ForeColor = Color.Black
        End If

        If TotalCompIPH < 0 Then
            lblBPCompIPH.ForeColor = Color.Red
        Else
            lblBPCompIPH.ForeColor = Color.Black
        End If

        ' ISK PER HOUR 
        lblBPRawIPH.Text = FormatNumber(TotalRawIPH, 2) ' Build everything
        lblBPCompIPH.Text = FormatNumber(TotalCompIPH, 2) ' Buy components

        ' Set the labels if the User Can make this item and/or all components
        If SelectedBlueprint.UserCanBuildBlueprint Then
            lblBPCanMakeBP.Text = "Can make this Item"
            lblBPCanMakeBP.ForeColor = Color.Black
        Else
            lblBPCanMakeBP.Text = "Cannot make this Item"
            lblBPCanMakeBP.ForeColor = Color.Red
        End If

        ' Only update the make all lable if we have something to make, else use the bp data
        If SelectedBlueprint.HasComponents Then
            If SelectedBlueprint.UserCanBuildAllComponents Then
                lblBPCanMakeBPAll.Text = "Can make All Components for this Item"
                lblBPCanMakeBPAll.ForeColor = Color.Black
            Else
                lblBPCanMakeBPAll.Text = "Cannot make All Components for this Item"
                lblBPCanMakeBPAll.ForeColor = Color.Red
            End If

            ' Has components, but if we are buying everything (no skills/build buy) - then state that instead, else show BP stuff
            If SelectedBlueprint.GetReqComponentSkills.NumSkills = 0 And chkBPBuildBuy.Checked Then
                lblBPCanMakeBPAll.Text = "Buying all Materials"
                lblBPCanMakeBPAll.ForeColor = Color.Black
            End If
        Else
            If SelectedBlueprint.UserCanBuildBlueprint Then
                lblBPCanMakeBPAll.Text = "Can make this Item"
                lblBPCanMakeBPAll.ForeColor = Color.Black
            Else
                lblBPCanMakeBPAll.Text = "Cannot make this Item"
                lblBPCanMakeBPAll.ForeColor = Color.Red
            End If

        End If

        ' BP production time
        lblBPProductionTime.Text = FormatIPHTime(SelectedBlueprint.GetProductionTime)
        ' Set the total time to produce all items for this Blueprint
        lblBPTotalItemPT.Text = FormatIPHTime(SelectedBlueprint.GetTotalProductionTime)

        ' SVR Values
        ' Set these first so it doesn't look goofy
        lblBPBPSVR.Text = "-"
        lblBPRawSVR.Text = "-"

        If UserApplicationSettings.AutoUpdateSVRonBPTab Then
            Dim TempBPSVR As String = GetBPItemSVR(SelectedBlueprint.GetProductionTime)
            Dim TempRawSVR As String = GetBPItemSVR(SelectedBlueprint.GetTotalProductionTime)
            ' Get the values before setting so they update at the same time on the form
            lblBPBPSVR.Text = TempBPSVR
            lblBPRawSVR.Text = TempRawSVR
        End If

        ' Set the ME and TE values if they changed
        txtBPME.Text = CStr(SelectedBlueprint.GetME)
        txtBPTE.Text = CStr(SelectedBlueprint.GetTE)

    End Sub

    Private Sub UpdateFacilityUsage(DivideUnits As Long)
        Dim UsedFacility As IndustryFacility = BPTabFacility.GetSelectedFacility()
        Dim TTText As String = ""

        ' Save all the usage values each time we update to allow updates for changing the facilit
        BPTabFacility.GetSelectedManufacturingFacility.FacilityUsage = SelectedBlueprint.GetManufacturingFacilityUsage / DivideUnits
        BPTabFacility.GetFacility(ProductionType.ComponentManufacturing).FacilityUsage = SelectedBlueprint.GetComponentFacilityUsage() / DivideUnits
        BPTabFacility.GetFacility(ProductionType.CapitalComponentManufacturing).FacilityUsage = SelectedBlueprint.GetCapComponentFacilityUsage() / DivideUnits
        BPTabFacility.GetFacility(ProductionType.Invention).FacilityUsage = SelectedBlueprint.GetInventionUsage() / DivideUnits
        BPTabFacility.GetFacility(ProductionType.Copying).FacilityUsage = SelectedBlueprint.GetCopyUsage() / DivideUnits

        ' Show the usage cost for the activity selected
        If UsedFacility.IncludeActivityUsage Then
            Select Case UsedFacility.Activity
                Case ManufacturingFacility.ActivityManufacturing
                    UsedFacility.FacilityUsage = SelectedBlueprint.GetManufacturingFacilityUsage / DivideUnits
                    TTText = GetUsageToolTipText(SelectedBlueprint.GetManufacturingFacility, True)
                Case ManufacturingFacility.ActivityInvention
                    UsedFacility.FacilityUsage = SelectedBlueprint.GetInventionUsage / DivideUnits
                    TTText = GetUsageToolTipText(SelectedBlueprint.GetInventionFacility, False)
                Case ManufacturingFacility.ActivityCopying
                    UsedFacility.FacilityUsage = SelectedBlueprint.GetCopyUsage / DivideUnits
                    TTText = GetUsageToolTipText(SelectedBlueprint.GetCopyFacility, False)
                Case ManufacturingFacility.ActivityComponentManufacturing
                    UsedFacility.FacilityUsage = SelectedBlueprint.GetComponentFacilityUsage / DivideUnits
                    TTText = GetUsageToolTipText(SelectedBlueprint.GetComponentManufacturingFacility, True)
                Case ManufacturingFacility.ActivityCapComponentManufacturing
                    UsedFacility.FacilityUsage = SelectedBlueprint.GetCapComponentFacilityUsage / DivideUnits
                    TTText = GetUsageToolTipText(SelectedBlueprint.GetCapitalComponentManufacturingFacility, True)
            End Select
        Else
            UsedFacility.FacilityUsage = 0
        End If

        ' Set the tool tip text
        Call BPTabFacility.UpdateUsage(TTText)

    End Sub

    ' Check if the runs they entered can be made with the number of blueprints, this only applies to BPC's (T2 and T3)
    Private Sub UpdateBPLinesandBPs()

        If Not IsNothing(SelectedBlueprint) Then
            If Trim(txtBPRuns.Text) <> "" Then
                txtBPNumBPs.Text = CStr(GetUsedNumBPs(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, CInt(txtBPRuns.Text),
                                                      CInt(txtBPLines.Text), CInt(txtBPNumBPs.Text), SelectedDecryptor.RunMod))
            End If
        End If

    End Sub

    ' Returns the number of BPs to use for item type and runs sent
    Private Function GetUsedNumBPs(ByVal BlueprintTypeID As Long, ByVal SentTechLevel As Integer,
                                   ByVal SentRuns As Integer, ByVal SentLines As Integer, ByVal SentNumBps As Integer, ByVal DecryptorMod As Integer) As Integer
        Dim readerOwned As SQLiteDataReader
        Dim SQL As String
        Dim MaxProductionRuns As Long
        Dim ReturnValue As Integer

        If SentTechLevel = 1 Then
            Return SentNumBps
        End If

        ' Set the number of bps
        If SentTechLevel = 2 Then
            SQL = "SELECT MAX_PRODUCTION_LIMIT FROM ALL_BLUEPRINTS WHERE BLUEPRINT_ID =" & CStr(BlueprintTypeID)

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerOwned = DBCommand.ExecuteReader()
            readerOwned.Read()

            MaxProductionRuns = readerOwned.GetInt32(0)

            readerOwned.Close()
            readerOwned = Nothing

        Else ' base T3 runs off of the relic
            Dim readerBP As SQLiteDataReader

            SQL = "SELECT quantity FROM INVENTORY_TYPES, INDUSTRY_ACTIVITY_PRODUCTS "
            SQL = SQL & "WHERE typeID = blueprintTypeID AND productTypeID = " & CStr(BlueprintTypeID) & " AND typeName = '" & cmbBPRelic.Text & "'"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerBP = DBCommand.ExecuteReader()

            If readerBP.Read Then
                MaxProductionRuns = readerBP.GetInt32(0)
            Else
                ' Assume wrecked bp
                MaxProductionRuns = 3
            End If

            readerBP.Close()
            readerBP = Nothing

        End If

        MaxProductionRuns = MaxProductionRuns + DecryptorMod
        ' Set the num bps off of the calculated amount
        ReturnValue = CInt(Math.Ceiling(SentRuns / MaxProductionRuns))

        Return ReturnValue

    End Function

    ' Adds item to shopping list
    Private Sub btnAddBPMatstoShoppingList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBPAddBPMatstoShoppingList.Click

        ' Just add it to shopping list with options
        Call AddToShoppingList(SelectedBlueprint, chkBPBuildBuy.Checked, rbtnBPRawmatCopy.Checked, BPTabFacility.GetFacility(BPTabFacility.GetCurrentFacilityProductionType()),
                               chkBPIgnoreInvention.Checked, chkBPIgnoreMinerals.Checked, chkBPIgnoreT1Item.Checked, rbtnBPCopyInvREMats.Checked)

        If TotalShoppingList.GetNumShoppingItems > 0 Then
            ' Add the final item and mark as items in list
            pnlShoppingList.Text = "Items in Shopping List"
            pnlShoppingList.ForeColor = Color.Red
        Else
            pnlShoppingList.Text = "No Items in Shopping List"
            pnlShoppingList.ForeColor = Color.Black
        End If

        ' Refresh the data if it's open
        If frmShop.Visible Then
            Call frmShop.RefreshLists()
        End If

    End Sub

    ' Loads the previous blueprint 
    Private Sub LoadPreviousBlueprint()
        Call LoadBPfromHistory(CurrentBPHistoryIndex - 1, "Backward")
    End Sub

    ' Loads the next blueprint if they used previous 
    Private Sub LoadNextBlueprint()
        Call LoadBPfromHistory(CurrentBPHistoryIndex + 1, "Forward")
    End Sub

    Private Sub LoadBPfromHistory(ByRef LocationID As Integer, BPType As String)

        If BPHistory.Count > 0 And LocationID < BPHistory.Count And LocationID >= 0 Then
            With BPHistory(LocationID)
                LoadingBPfromHistory = True
                Call LoadBPfromEvent(.BPID, .BuildType, .Inputs, .SentFrom, .BuildFacility, .ComponentFacility, .CapComponentFacility, .InventionFacility, .CopyFacility, .IncludeTaxes,
                                           .IncludeFees, .MEValue, .TEValue, .SentRuns, .ManufacturingLines, .LabLines, .NumBPs, .AddlCosts, .PPU)
            End With

            CurrentBPHistoryIndex = LocationID

            Call UpdateBlueprintHistoryButtons()
        End If
    End Sub

    Private Sub UpdateBlueprintHistoryButtons()
        If BPHistory.Count > 1 Then
            If CurrentBPHistoryIndex = 0 Then
                ' Switch back to transparent until they go forward
                btnBPBack.BackColor = Color.Transparent
            Else
                btnBPBack.BackColor = Color.SteelBlue
            End If

            If CurrentBPHistoryIndex = BPHistory.Count - 1 Then
                btnBPForward.BackColor = Color.Transparent
            Else
                btnBPForward.BackColor = Color.SteelBlue
            End If
        Else
            btnBPBack.BackColor = Color.Transparent
            btnBPForward.BackColor = Color.Transparent
        End If
    End Sub

    ' Takes the facility and sets all the tool tip text based on the data it used
    Private Function GetUsageToolTipText(SentFacility As IndustryFacility, IncludeTax As Boolean) As String
        ' Set the usage tool tip data
        Dim TTString As String = ""

        TTString = TTString & "System Index = " & FormatPercent(SentFacility.CostIndex, 2) & " " & vbCrLf
        If IncludeTax Then
            TTString = TTString & "Facility Tax Rate = " & FormatPercent(SentFacility.TaxRate, 2) & " " & vbCrLf
        End If
        TTString = TTString & "Double-click for a list of facility usages"

        Return TTString

    End Function

    ' Updates the cost index in the DB or adds it if it doesn't exist
    Private Sub btnBPUpdateCostIndex_Click(sender As System.Object, e As System.EventArgs) Handles btnBPUpdateCostIndex.Click
        ' Check the data
        Dim Text As String = txtBPUpdateCostIndex.Text.Replace("%", "")
        Dim SelectedFacility As IndustryFacility = BPTabFacility.GetSelectedFacility
        Dim SelectedActivity As String = BPTabFacility.GetSelectedFacility.Activity

        If Text <> "" Then
            If Not IsNumeric(Text) Then
                MsgBox("Invalid Cost index value", vbExclamation, Application.ProductName)
                txtBPUpdateCostIndex.Focus()
                Exit Sub
            End If
        End If

        Application.UseWaitCursor = True
        Application.DoEvents()

        Dim SQL As String
        Dim rsCheck As SQLiteDataReader
        Dim SolarSystemName As String = Trim(FormatDBString(SelectedFacility.SolarSystemName.Substring(0, InStr(SelectedFacility.SolarSystemName, "(") - 1)))
        Dim CostIndex As String = CStr(Val(txtBPUpdateCostIndex.Text.Replace("%", "")) / 100)

        ' Look up Solar System ID
        Dim SSID As String = CStr(GetSolarSystemID(SolarSystemName))
        Dim TempActivityID As String = ""

        Select Case SelectedActivity
            Case ManufacturingFacility.ActivityManufacturing ', ManufacturingFacility.ActivityComponentManufacturing, ManufacturingFacility.ActivityCapComponentManufacturing
                TempActivityID = "1"
            Case ManufacturingFacility.ActivityCopying
                TempActivityID = "5"
            Case ManufacturingFacility.ActivityInvention
                TempActivityID = "8"
            Case Else
                TempActivityID = "1"
        End Select

        SQL = "SELECT * FROM INDUSTRY_SYSTEMS_COST_INDICIES WHERE SOLAR_SYSTEM_ID = " & SSID & " "

        SQL = SQL & "AND ACTIVITY_NAME = '" & SelectedFacility.Activity & "'"
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsCheck = DBCommand.ExecuteReader
        rsCheck.Read()

        ' See if the Current facility data exists, and update it. If not, then insert a new record
        If rsCheck.HasRows Then
            ' Update
            SQL = "UPDATE INDUSTRY_SYSTEMS_COST_INDICIES SET COST_INDEX = " & CostIndex
            SQL = SQL & " WHERE SOLAR_SYSTEM_ID = " & SSID & " AND ACTIVITY_NAME = '" & SelectedActivity & "'"
        Else
            ' Insert 
            SQL = "INSERT INTO INDUSTRY_SYSTEMS_COST_INDICIES VALUES (" & SSID & ",'" & SolarSystemName & "'," & TempActivityID & ",'"
            SQL = SQL & SelectedActivity & "'," & CostIndex & ")"

        End If

        rsCheck.Close()
        DBCommand = Nothing

        Call EVEDB.ExecuteNonQuerySQL(SQL)

        ' Update the station records for this system
        SQL = "UPDATE STATION_FACILITIES SET COST_INDEX = " & CostIndex
        SQL = SQL & " WHERE SOLAR_SYSTEM_ID = " & SSID & " AND ACTIVITY_ID = " & TempActivityID

        Call EVEDB.ExecuteNonQuerySQL(SQL)

        ' Reload all the facilities to get the change
        Call BPTabFacility.InitializeControl(FacilityView.FullControls, SelectedCharacter.ID, ProgramLocation.BlueprintTab, BPTabFacility.GetCurrentFacilityProductionType)

        ' Refresh the bp, which will reload the facility with the changes
        Call UpdateBPGrids(SelectedBlueprint.GetTypeID, SelectedBlueprint.GetTechLevel, False, SelectedBlueprint.GetItemGroupID, SelectedBlueprint.GetItemCategoryID, SentFromLocation.BlueprintTab)

        btnBPUpdateCostIndex.Enabled = False

        Application.UseWaitCursor = False
        Application.DoEvents()
        MsgBox("Index Updated", vbInformation, Application.ProductName)

    End Sub

    ' Allow update of the market price from clicking on the label
    Private Sub lblBPMarketCost_Click(sender As System.Object, e As System.EventArgs) Handles lblBPMarketCost.Click
        txtBPMarketPriceEdit.Size = lblBPMarketCost.Size
        Dim p As Point = lblBPMarketCost.Location
        p.Y = lblBPMarketCost.Location.Y - 2 ' move the text box up two pixels since it is larger than the label and looks funky
        txtBPMarketPriceEdit.Location = p
        txtBPMarketPriceEdit.Text = lblBPMarketCost.Text
        txtBPMarketPriceEdit.Visible = True
        txtBPMarketPriceEdit.Focus()
    End Sub

    Private Function DoesBPHaveBuildableComponents(BPID As Long) As Boolean
        Dim SQL As String
        Dim readerBP As SQLiteDataReader

        ' See if this has buildable components
        SQL = "SELECT DISTINCT 'X' FROM ALL_BLUEPRINTS "
        SQL &= "WHERE ITEM_ID IN (SELECT MATERIAL_ID FROM ALL_BLUEPRINT_MATERIALS WHERE BLUEPRINT_ID = {0})"
        DBCommand = New SQLiteCommand(String.Format(SQL, BPID), EVEDB.DBREf)
        readerBP = DBCommand.ExecuteReader

        If readerBP.Read Then
            Return True
        Else
            Return False
        End If

    End Function

#End Region

#Region "Update Prices Tab"

#Region "Update Prices Tab User Object (Check boxes, Text, Buttons) Functions/Procedures "

    ' Disables the forms and controls on update prices
    Private Sub DisableUpdatePricesTab(Value As Boolean)
        ' Disable tab
        gbRawMaterials.Enabled = Not Value
        gbManufacturedItems.Enabled = Not Value
        gbRegions.Enabled = Not Value
        gbTradeHubSystems.Enabled = Not Value
        gbPriceOptions.Enabled = Not Value
        txtPriceItemFilter.Enabled = Not Value
        lblItemFilter.Enabled = Not Value
        btnClearItemFilter.Enabled = Not Value
        chkPriceRawMaterialPrices.Enabled = Not Value
        chkPriceManufacturedPrices.Enabled = Not Value
        btnToggleAllPriceItems.Enabled = Not Value
        btnDownloadPrices.Enabled = Not Value
        btnSaveUpdatePrices.Enabled = Not Value
        lstPricesView.Enabled = Not Value
        btnSavePricestoFile.Enabled = Not Value
        btnLoadPricesfromFile.Enabled = Not Value
    End Sub

    ' Checks or unchecks all the prices
    Private Sub UpdateAllPrices()
        If RunUpdatePriceList Then
            ' Don't update prices yet
            UpdateAllTechChecks = True
            RunUpdatePriceList = False

            Application.DoEvents()

            ' Just update the prices based on the checks
            Call CheckAllManufacturedPrices()
            Call CheckAllRawPrices()

            ' Good to go, update or clear
            RunUpdatePriceList = True
            UpdateAllTechChecks = True

            Application.DoEvents()

            If chkPriceManufacturedPrices.Checked = False And chkPriceRawMaterialPrices.Checked = False Then
                lstPricesView.Items.Clear()
                btnToggleAllPriceItems.Text = "Select All Items"
            Else
                If chkPriceManufacturedPrices.Checked = True And chkPriceRawMaterialPrices.Checked = True Then
                    btnToggleAllPriceItems.Text = "Uncheck All Items"
                Else
                    btnToggleAllPriceItems.Text = "Select All Items"
                End If
                Call UpdatePriceList()
            End If
        End If
    End Sub

    ' Checks or unchecks just the prices for raw material items
    Private Sub CheckAllRawPrices()

        RunUpdatePriceList = False

        ' Check all item boxes and do not run updates
        If chkPriceRawMaterialPrices.Checked = True Then
            chkMinerals.Checked = True
            chkIceProducts.Checked = True
            chkGas.Checked = True
            chkAbyssalMaterials.Checked = True
            chkBPCs.Checked = True
            chkMisc.Checked = True
            chkAncientRelics.Checked = True
            chkAncientSalvage.Checked = True
            chkSalvage.Checked = True
            chkPlanetary.Checked = True
            chkDatacores.Checked = True
            chkDecryptors.Checked = True
            chkRawMats.Checked = True
            chkProcessedMats.Checked = True
            chkAdvancedMats.Checked = True
            chkMatsandCompounds.Checked = True
            chkDroneComponents.Checked = True
            chkBoosterMats.Checked = True
            chkPolymers.Checked = True
            chkAsteroids.Checked = True
        Else ' Turn off all item checks
            chkMinerals.Checked = False
            chkIceProducts.Checked = False
            chkGas.Checked = False
            chkAbyssalMaterials.Checked = False
            chkBPCs.Checked = False
            chkMisc.Checked = False
            chkAncientRelics.Checked = False
            chkAncientSalvage.Checked = False
            chkSalvage.Checked = False
            chkPlanetary.Checked = False
            chkDatacores.Checked = False
            chkDecryptors.Checked = False
            chkRawMats.Checked = False
            chkProcessedMats.Checked = False
            chkAdvancedMats.Checked = False
            chkMatsandCompounds.Checked = False
            chkDroneComponents.Checked = False
            chkBoosterMats.Checked = False
            chkPolymers.Checked = False
            chkAsteroids.Checked = False
        End If

        RunUpdatePriceList = True

    End Sub

    ' Checks or unchecks just the prices for manufactured items
    Private Sub CheckAllManufacturedPrices()

        RunUpdatePriceList = False

        ' Check all item boxes and do not run updates
        If chkPriceManufacturedPrices.Checked = True Then
            chkShips.Checked = True
            chkModules.Checked = True
            chkDrones.Checked = True
            chkBoosters.Checked = True
            chkRigs.Checked = True
            chkCharges.Checked = True
            chkSubsystems.Checked = True
            chkStructures.Checked = True
            chkTools.Checked = True
            chkCapT2Components.Checked = True
            chkCapitalComponents.Checked = True
            chkComponents.Checked = True
            chkHybrid.Checked = True
            chkFuelBlocks.Checked = True
            chkStructureRigs.Checked = True
            chkCelestials.Checked = True
            chkDeployables.Checked = True
            chkImplants.Checked = True
            chkStructureModules.Checked = True
        Else ' Turn off all item checks
            chkShips.Checked = False
            chkModules.Checked = False
            chkDrones.Checked = False
            chkBoosters.Checked = False
            chkRigs.Checked = False
            chkCharges.Checked = False
            chkSubsystems.Checked = False
            chkStructures.Checked = False
            chkTools.Checked = False
            chkCapT2Components.Checked = False
            chkCapitalComponents.Checked = False
            chkComponents.Checked = False
            chkHybrid.Checked = False
            chkFuelBlocks.Checked = False
            chkStructureRigs.Checked = False
            chkCelestials.Checked = False
            chkDeployables.Checked = False
            chkImplants.Checked = False
            chkStructureModules.Checked = False
        End If

        RunUpdatePriceList = True

    End Sub

    Private Sub chkPriceSelectManufacturedItems_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkPriceManufacturedPrices.CheckedChanged

        Call CheckAllManufacturedPrices()

        If chkPriceManufacturedPrices.Checked = False And chkPriceRawMaterialPrices.Checked = False Then
            lstPricesView.Items.Clear()
            btnToggleAllPriceItems.Text = "Select All Items"
        Else
            If chkPriceManufacturedPrices.Checked = True And chkPriceRawMaterialPrices.Checked = True Then
                btnToggleAllPriceItems.Text = "Uncheck All Items"
            Else
                btnToggleAllPriceItems.Text = "Select All Items"
            End If
        End If

        If PriceToggleButtonHit = False And Not FirstLoad Then
            Call UpdatePriceList()
        End If

    End Sub

    Private Sub chkPriceRawMaterialPrices_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkPriceRawMaterialPrices.CheckedChanged

        Call CheckAllRawPrices()

        If chkPriceManufacturedPrices.Checked = False And chkPriceRawMaterialPrices.Checked = False Then
            lstPricesView.Items.Clear()
            btnToggleAllPriceItems.Text = "Select All Items"
        Else
            If chkPriceManufacturedPrices.Checked = True And chkPriceRawMaterialPrices.Checked = True Then
                btnToggleAllPriceItems.Text = "Uncheck All Items"
            Else
                btnToggleAllPriceItems.Text = "Select All Items"
            End If
        End If

        If PriceToggleButtonHit = False And Not FirstLoad Then
            Call UpdatePriceList()
        End If

    End Sub

    ' Toggles all selection checks on the prices tab
    Private Sub btnToggleAllPriceItems_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleAllPriceItems.Click

        RunUpdatePriceList = False
        PriceToggleButtonHit = True

        If btnToggleAllPriceItems.Text = "Select All Items" And (chkPriceManufacturedPrices.Checked = False Or chkPriceRawMaterialPrices.Checked = False) Then
            ' Set the name, then uncheck all
            btnToggleAllPriceItems.Text = "Uncheck All Items"
            chkPriceRawMaterialPrices.Checked = True
            chkPriceManufacturedPrices.Checked = True
        ElseIf btnToggleAllPriceItems.Text = "Uncheck All Items" And chkPriceManufacturedPrices.Checked = True And chkPriceRawMaterialPrices.Checked = True Then
            ' Turn off all item checks
            btnToggleAllPriceItems.Text = "Select All Items"
            chkPriceRawMaterialPrices.Checked = False
            chkPriceManufacturedPrices.Checked = False
        End If

        RunUpdatePriceList = True

        Call UpdateAllPrices()
        PriceToggleButtonHit = False

    End Sub

    ' EVE Central Link
    Private Sub llblEVEMarketerContribute_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        System.Diagnostics.Process.Start("http://eve-central.com/home/software.html")
    End Sub

    ' Updates the T1, T2 and T3 check boxes depending on item selections
    Private Sub UpdateTechChecks()
        Dim T1 As Boolean = False
        Dim T2 As Boolean = False
        Dim T3 As Boolean = False
        Dim Storyline As Boolean = False
        Dim Navy As Boolean = False
        Dim Pirate As Boolean = False

        Dim ItemsSelected As Boolean = False
        Dim i As Integer
        Dim TechChecks As Boolean = False

        ' For check all 
        If Not RunUpdatePriceList And UpdateAllTechChecks Then
            UpdateAllTechChecks = False
            ' Check all and leave
            For i = 1 To TechCheckBoxes.Length - 1
                TechCheckBoxes(i).Enabled = True
                ' Check this one and leave
                TechCheckBoxes(i).Checked = True
            Next i
            Exit Sub
        End If

        ' Check each item checked and set the check boxes accordingly
        If chkShips.Checked Then
            T1 = True
            T2 = True
            T3 = True
            Navy = True
            Pirate = True
            ItemsSelected = True
        End If

        If chkModules.Checked Then
            T1 = True
            T2 = True
            Navy = True
            Storyline = True
            ItemsSelected = True
        End If

        If chkSubsystems.Checked Then
            T3 = True
            ItemsSelected = True
        End If

        If chkDrones.Checked Then
            T1 = True
            T2 = True
            ItemsSelected = True
        End If

        If chkRigs.Checked Then
            T1 = True
            T2 = True
            ItemsSelected = True
        End If

        If chkBoosters.Checked Then
            T1 = True
            ItemsSelected = True
        End If

        If chkStructures.Checked Then
            T1 = True
            Pirate = True
            ItemsSelected = True
        End If

        If chkCharges.Checked Then
            T1 = True
            T2 = True
            ItemsSelected = True
        End If

        ' If none are checked, then uncheck and un-enable all
        If ItemsSelected Then

            ' Enable the Checks
            If T1 Then
                chkPricesT1.Enabled = True
            Else
                chkPricesT1.Enabled = False
            End If

            If T2 Then
                chkPricesT2.Enabled = True
            Else
                chkPricesT2.Enabled = False
            End If

            If T3 Then
                chkPricesT3.Enabled = True
            Else
                chkPricesT3.Enabled = False
            End If

            If Storyline Then
                chkPricesT4.Enabled = True
            Else
                chkPricesT4.Enabled = False
            End If

            If Navy Then
                chkPricesT5.Enabled = True
            Else
                chkPricesT5.Enabled = False
            End If

            If Pirate Then
                chkPricesT6.Enabled = True
            Else
                chkPricesT6.Enabled = False
            End If

            ' Make sure we have at le=t one checked
            For i = 1 To TechCheckBoxes.Length - 1
                If TechCheckBoxes(i).Enabled Then
                    If TechCheckBoxes(i).Checked Then
                        TechChecks = True
                        ' Found one enabled and checked, so leave for
                        Exit For
                    End If
                End If
            Next i

            If Not TechChecks Then
                ' Need to check at le=t one
                For i = 1 To TechCheckBoxes.Length - 1
                    If TechCheckBoxes(i).Enabled Then
                        ' Check this one and leave
                        TechCheckBoxes(i).Checked = True
                    End If
                Next i
            End If

        Else
            chkPricesT1.Enabled = False
            chkPricesT2.Enabled = False
            chkPricesT3.Enabled = False
            chkPricesT4.Enabled = False
            chkPricesT5.Enabled = False
            chkPricesT6.Enabled = False
        End If

        ' Save status of the Tech check boxes
        PriceCheckT1Enabled = chkPricesT1.Enabled
        PriceCheckT2Enabled = chkPricesT2.Enabled
        PriceCheckT3Enabled = chkPricesT3.Enabled
        PriceCheckT4Enabled = chkPricesT4.Enabled
        PriceCheckT5Enabled = chkPricesT5.Enabled
        PriceCheckT6Enabled = chkPricesT6.Enabled

    End Sub

    ' Clears all system's that may be checked including resetting the system combo
    Private Sub ClearSystemChecks(Optional ResetSystemCombo As Boolean = True)
        Dim i As Integer

        If Not IgnoreSystemCheckUpdates Then
            For i = 1 To SystemCheckBoxes.Length - 1
                SystemCheckBoxes(i).Checked = False
            Next
            ' Reset the system combo
            If ResetSystemCombo Then
                cmbPriceSystems.Text = DefaultSystemPriceCombo
            End If
        End If
    End Sub

    ' Function clears all region check boxes - if an index is sent, it will uncheck them all unless it's not -1 and leave it checked for single region selection
    Private Sub ClearAllRegionChecks(ByVal Index As Integer)
        Dim i As Integer

        If Not IgnoreRegionCheckUpdates Then
            For i = 1 To RegionCheckBoxes.Length - 1
                If i <> Index Then
                    RegionCheckBoxes(i).Checked = False
                End If
            Next i
        End If
    End Sub

    Private Sub cmbPriceShipTypes_DropDown(sender As Object, e As System.EventArgs) Handles cmbPriceShipTypes.DropDown
        If FirstPriceShipTypesComboLoad Then
            Call LoadPriceShipTypes()
            FirstPriceShipTypesComboLoad = False
        End If
    End Sub

    Private Sub cmbPriceChargeTypes_DropDown(sender As Object, e As System.EventArgs) Handles cmbPriceChargeTypes.DropDown
        If FirstPriceChargeTypesComboLoad Then
            Call LoadPriceChargeTypes()
            FirstPriceChargeTypesComboLoad = False
        End If
    End Sub

    Private Sub txtPriceItemFilter_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtPriceItemFilter.KeyDown
        'Call ProcessCutCopyPasteSelect(txtPriceItemFilter, e)
        If e.KeyCode = Keys.Enter Then
            Call UpdatePriceList()
        End If
    End Sub

    ' Checks all item check's to see if there is one checked. True if one or more checked, False if not
    Private Function ItemsSelected() As Boolean

        ' If the prices list doesnt' have any items in it, nothing to update so nothing checked
        If lstPricesView.Items.Count <> 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Sub chkPricesT1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPricesT1.Click
        If RefreshList Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub chkPricesT2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPricesT2.Click
        If RefreshList Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub chkPricesT3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPricesT3.Click
        If RefreshList Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub chkPricesT4_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPricesT4.Click
        If RefreshList Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub chkPricesT5_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPricesT5.Click
        If RefreshList Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub chkPricesT6_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPricesT6.Click
        If RefreshList Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub chkMinerals_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMinerals.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkIceProducts_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkIceProducts.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkDataCores_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDatacores.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkDecryptors_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDecryptors.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkGas_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkGas.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkAbyssalMaterials_CheckedChanged(sender As Object, e As EventArgs) Handles chkAbyssalMaterials.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkBlueprints_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkBPCs.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkMisc_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMisc.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkSalvage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSalvage.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkAncientSalvage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAncientSalvage.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkAncientRelics_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAncientRelics.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkPolymers_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPolymers.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkRawMats_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRawMats.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkPlanetary_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkPlanetary.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkAsteroids_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkAsteroids.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkProcessedMats_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkProcessedMats.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkAdvancedMats_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAdvancedMats.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkMatsandCompounds_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMatsandCompounds.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkDroneComponents_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDroneComponents.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkStructureRigs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkStructureRigs.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkDeployables_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkDeployables.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkStructureModules_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkStructureModules.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkCelestial_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCelestials.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkImplants_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkImplants.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkBoosterMats_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBoosterMats.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkTools_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkTools.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkFuelBlocks_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkFuelBlocks.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkDataInterfaces_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call UpdatePriceList()
    End Sub

    Private Sub chkHybrid_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkHybrid.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkComponents_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkComponents.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkCapitalComponents_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCapitalComponents.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkCapT2Components_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCapT2Components.CheckedChanged
        Call UpdatePriceList()
    End Sub

    Private Sub chkUpdatePricesUseESI_CheckedChanged(sender As System.Object, e As System.EventArgs)
        If Not FirstLoad Then
            Call ClearAllRegionChecks(0)
        End If
    End Sub

    Private Sub chkBoosters_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBoosters.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkRigs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRigs.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkShips_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShips.CheckedChanged

        If chkShips.Checked = True Then
            cmbPriceShipTypes.Enabled = True
        ElseIf chkShips.Checked = False Then
            cmbPriceShipTypes.Enabled = False
        End If

        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True

    End Sub

    Private Sub chkModules_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkModules.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkDrones_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDrones.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkCharges_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCharges.CheckedChanged

        If chkCharges.Checked = True Then
            cmbPriceChargeTypes.Enabled = True
        ElseIf chkCharges.Checked = False Then
            cmbPriceChargeTypes.Enabled = False
        End If

        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True

    End Sub

    Private Sub chkSubsystems_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSubsystems.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkStructures_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkStructures.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkUpdatPricesNoPrice_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUpdatePricesNoPrice.CheckedChanged
        RefreshList = False
        Call UpdateTechChecks()
        Call UpdatePriceList()
        RefreshList = True
    End Sub

    Private Sub chkSystems1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkSystems1.CheckedChanged
        Call SyncPriceCheckBoxes(1)
    End Sub

    Private Sub chkSystems2_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkSystems2.CheckedChanged
        Call SyncPriceCheckBoxes(2)
    End Sub

    Private Sub chkSystems3_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkSystems3.CheckedChanged
        Call SyncPriceCheckBoxes(3)
    End Sub

    Private Sub chkSystems4_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkSystems4.CheckedChanged
        Call SyncPriceCheckBoxes(4)
    End Sub

    Private Sub chkSystems5_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkSystems5.CheckedChanged
        Call SyncPriceCheckBoxes(5)
    End Sub

    Private Sub cmbPriceSystems_DropDown(sender As Object, e As System.EventArgs) Handles cmbPriceSystems.DropDown
        ' If you drop down, don't show the text window
        cmbPriceSystems.AutoCompleteMode = AutoCompleteMode.None

        If FirstSolarSystemComboLoad Then
            Call LoadPriceSolarSystems()
            FirstSolarSystemComboLoad = False
        End If
    End Sub

    Private Sub cmbPriceSystems_DropDownClosed(sender As Object, e As System.EventArgs) Handles cmbPriceSystems.DropDownClosed
        ' If it closes up, re-enable autocomplete
        cmbPriceSystems.AutoCompleteMode = AutoCompleteMode.SuggestAppend
    End Sub

    Private Sub cmbPriceSystems_GotFocus(sender As Object, e As System.EventArgs) Handles cmbPriceSystems.GotFocus
        cmbPriceSystems.SelectAll()
    End Sub

    Private Sub cmbPriceSystems_SelectionChangeCommitted(sender As Object, e As EventArgs) Handles cmbPriceSystems.SelectionChangeCommitted
        If cmbPriceSystems.Text <> DefaultSystemPriceCombo Then
            Call ClearSystemChecks(False)
            Call ClearAllRegionChecks(0)
        End If
    End Sub

    Private Sub cmbPriceSystems_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbPriceSystems.SelectedIndexChanged
        If cmbPriceSystems.Text <> DefaultSystemPriceCombo Then
            Call ClearSystemChecks(False)
            Call ClearAllRegionChecks(0)
        End If
    End Sub

    Private Sub cmbPriceShipTypes_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbPriceShipTypes.SelectedIndexChanged
        If Not FirstPriceShipTypesComboLoad Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub cmbPriceChargeTypes_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbPriceChargeTypes.SelectedIndexChanged
        If Not FirstPriceChargeTypesComboLoad Then
            Call UpdatePriceList()
        End If
    End Sub

    Private Sub SyncPriceCheckBoxes(ByVal TriggerIndex As Integer)
        Dim i As Integer

        If Not FirstLoad Then
            ' Trigger Index is a box that was checked on or off
            If SystemCheckBoxes(TriggerIndex).Checked = True Then
                ' Uncheck all other systems and regions
                For i = 1 To SystemCheckBoxes.Length - 1
                    If i <> TriggerIndex Then
                        SystemCheckBoxes(i).Checked = False
                    End If
                Next
                ' Uncheck regions
                Call ClearAllRegionChecks(0)
                ' Reset the solar system combo
                cmbPriceSystems.Text = DefaultSystemPriceCombo
            End If
        End If

    End Sub

    Private Sub lstPricesView_ColumnClick(sender As System.Object, e As System.Windows.Forms.ColumnClickEventArgs) Handles lstPricesView.ColumnClick

        Call ListViewColumnSorter(e.Column, CType(lstPricesView, ListView), UpdatePricesColumnClicked, UpdatePricesColumnSortType)

    End Sub

    Private Sub rbtnPriceSettingPriceProfile_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbtnPriceSettingPriceProfile.CheckedChanged
        ' set in init, and use this to toggle
        If rbtnPriceSettingPriceProfile.Checked Then
            pnlPriceProfiles.Visible = True
            pnlSinglePriceLocationSelect.Visible = False
            ' Disable other buttons and lists
            cmbRawMatsSplitPrices.Enabled = False
            lblRawMatsSplitPrices.Enabled = False
            cmbItemsSplitPrices.Enabled = False
            lblItemsSplitPrices.Enabled = False
            lblRawPriceModifier.Enabled = False
            lblItemsPriceModifier.Enabled = False
            txtRawPriceModifier.Enabled = False
            txtItemsPriceModifier.Enabled = False
        Else
            pnlPriceProfiles.Visible = False
            pnlSinglePriceLocationSelect.Visible = True
            ' Enable other buttons and lists
            cmbRawMatsSplitPrices.Enabled = True
            lblRawMatsSplitPrices.Enabled = True
            cmbItemsSplitPrices.Enabled = True
            lblItemsSplitPrices.Enabled = True
            lblRawPriceModifier.Enabled = True
            lblItemsPriceModifier.Enabled = True
            txtRawPriceModifier.Enabled = True
            txtItemsPriceModifier.Enabled = True
        End If
    End Sub

    Private Sub txtRawPriceModifier_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtRawPriceModifier.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedNegativePercentChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtItemsPriceModifier_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtItemsPriceModifier.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedNegativePercentChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtRawPriceModifier_LostFocus(sender As Object, e As System.EventArgs) Handles txtRawPriceModifier.LostFocus
        If Trim(txtRawPriceModifier.Text) = "" Then
            txtRawPriceModifier.Text = "0.0%"
        Else
            txtRawPriceModifier.Text = FormatPercent(CDbl(txtRawPriceModifier.Text.Replace("%", "")) / 100, 1)
        End If
    End Sub

    Private Sub txtItemsPriceModifier_LostFocus(sender As Object, e As System.EventArgs) Handles txtItemsPriceModifier.LostFocus
        If Trim(txtItemsPriceModifier.Text) = "" Then
            txtItemsPriceModifier.Text = "0.0%"
        Else
            txtItemsPriceModifier.Text = FormatPercent(CDbl(txtItemsPriceModifier.Text.Replace("%", "")) / 100, 1)
        End If
    End Sub

    Private Sub txtRawMaterialsDefaultsPriceMod_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtRawMaterialsDefaultsPriceMod.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedNegativePercentChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtItemsDefaultsPriceMod_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtItemsDefaultsPriceMod.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedNegativePercentChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub cmbItemsDefaultsRegion_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbItemsDefaultsRegion.SelectedIndexChanged
        If DefaultPreviousItemsRegion <> cmbItemsDefaultsRegion.Text Then
            PPItemsSystemsLoaded = False
            cmbItemsDefaultsSystem.Text = AllSystems
            DefaultPreviousItemsRegion = cmbItemsDefaultsRegion.Text
        End If
    End Sub

    Private Sub cmbRawMaterialsDefaultsRegion_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbRawMaterialsDefaultsRegion.SelectedIndexChanged
        If DefaultPreviousRawRegion <> cmbRawMaterialsDefaultsRegion.Text Then
            PPRawSystemsLoaded = False
            cmbRawMaterialsDefaultsSystem.Text = AllSystems
            DefaultPreviousRawRegion = cmbRawMaterialsDefaultsRegion.Text
        End If
    End Sub

    Private Sub cmbRawMaterialsDefaultsSystem_DropDown(sender As System.Object, e As System.EventArgs) Handles cmbRawMaterialsDefaultsSystem.DropDown
        If Not PPRawSystemsLoaded Then
            Call LoadPPDefaultsSystemCombo(cmbRawMaterialsDefaultsSystem, cmbRawMaterialsDefaultsRegion.Text, AllSystems)
        End If
    End Sub

    Private Sub cmbItemsDefaultsSystem_DropDown(sender As System.Object, e As System.EventArgs) Handles cmbItemsDefaultsSystem.DropDown
        If Not PPItemsSystemsLoaded Then
            Call LoadPPDefaultsSystemCombo(cmbItemsDefaultsSystem, cmbItemsDefaultsRegion.Text, AllSystems)
        End If
    End Sub

    Private Sub btnRawMaterialsDefaults_Click(sender As System.Object, e As System.EventArgs) Handles btnRawMaterialsDefaults.Click

        ' Do some error checking first
        If Trim(cmbRawMaterialsDefaultsRegion.Text) = "" Or Not cmbRawMaterialsDefaultsRegion.Items.Contains(cmbRawMaterialsDefaultsRegion.Text) Then
            MsgBox("Invalid Default Region", vbExclamation, Application.ProductName)
            cmbRawMaterialsDefaultsRegion.Focus()
            Exit Sub
        End If

        If Trim(cmbRawMaterialsDefaultsSystem.Text) = "" Or Not cmbRawMaterialsDefaultsSystem.Items.Contains(cmbRawMaterialsDefaultsSystem.Text) Then
            MsgBox("Invalid Default System", vbExclamation, Application.ProductName)
            cmbRawMaterialsDefaultsSystem.Focus()
            Exit Sub
        End If

        If Trim(txtRawMaterialsDefaultsPriceMod.Text) = "" Then
            MsgBox("Invalid Default Price Modifier", vbExclamation, Application.ProductName)
            txtRawMaterialsDefaultsPriceMod.Focus()
            Exit Sub
        End If

        Call SetPriceProfileDefaults(cmbRawMaterialsDefaultsPriceType.Text, cmbRawMaterialsDefaultsRegion.Text, cmbRawMaterialsDefaultsSystem.Text, txtRawMaterialsDefaultsPriceMod.Text, True)

        ' Save these defaults to settings
        UserUpdatePricesTabSettings.PPRawPriceType = cmbRawMaterialsDefaultsPriceType.Text
        UserUpdatePricesTabSettings.PPRawRegion = cmbRawMaterialsDefaultsRegion.Text
        UserUpdatePricesTabSettings.PPRawSystem = cmbRawMaterialsDefaultsSystem.Text
        UserUpdatePricesTabSettings.PPRawPriceMod = CDbl(txtRawMaterialsDefaultsPriceMod.Text.Replace("%", "")) / 100

        AllSettings.SaveUpdatePricesSettings(UserUpdatePricesTabSettings)

        ' Refresh the grids
        Call LoadPriceProfileGrids()

        MsgBox("Defaults set", vbInformation, Application.ProductName)

    End Sub

    Private Sub btnItemsDefaults_Click(sender As System.Object, e As System.EventArgs) Handles btnItemsDefaults.Click

        ' Do some error checking first
        If Trim(cmbItemsDefaultsRegion.Text) = "" Or Not cmbItemsDefaultsRegion.Items.Contains(cmbItemsDefaultsRegion.Text) Then
            MsgBox("Invalid Default Region", vbExclamation, Application.ProductName)
            cmbItemsDefaultsRegion.Focus()
            Exit Sub
        End If

        If Trim(cmbItemsDefaultsSystem.Text) = "" Or Not cmbItemsDefaultsSystem.Items.Contains(cmbItemsDefaultsSystem.Text) Then
            MsgBox("Invalid Default System", vbExclamation, Application.ProductName)
            cmbItemsDefaultsSystem.Focus()
            Exit Sub
        End If

        If Trim(txtItemsDefaultsPriceMod.Text) = "" Then
            MsgBox("Invalid Default Price Modifier", vbExclamation, Application.ProductName)
            txtItemsPriceModifier.Focus()
            Exit Sub
        End If

        Call SetPriceProfileDefaults(cmbItemsDefaultsPriceType.Text, cmbItemsDefaultsRegion.Text, cmbItemsDefaultsSystem.Text, txtItemsDefaultsPriceMod.Text, False)

        ' Save these defaults to settings
        UserUpdatePricesTabSettings.PPItemsPriceType = cmbItemsDefaultsPriceType.Text
        UserUpdatePricesTabSettings.PPItemsRegion = cmbItemsDefaultsRegion.Text
        UserUpdatePricesTabSettings.PPItemsSystem = cmbItemsDefaultsSystem.Text
        UserUpdatePricesTabSettings.PPItemsPriceMod = CDbl(txtItemsDefaultsPriceMod.Text.Replace("%", "")) / 100

        AllSettings.SaveUpdatePricesSettings(UserUpdatePricesTabSettings)

        ' Refresh the grids
        Call LoadPriceProfileGrids()

        MsgBox("Defaults set", vbInformation, Application.ProductName)

    End Sub

#Region "Update Price Region Checks"
    Private Sub chkRegion1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion1.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(1)
        End If
    End Sub
    Private Sub chkRegion2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion2.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(2)
        End If
    End Sub
    Private Sub chkRegion3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion3.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(3)
        End If
    End Sub
    Private Sub chkRegion4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion4.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(4)
        End If
    End Sub
    Private Sub chkRegion5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion5.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(5)
        End If
    End Sub
    Private Sub chkRegion6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion6.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(6)
        End If
    End Sub
    Private Sub chkRegion7_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion7.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(7)
        End If
    End Sub
    Private Sub chkRegion8_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion8.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(8)
        End If
    End Sub
    Private Sub chkRegion9_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion9.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(9)
        End If
    End Sub
    Private Sub chkRegion10_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion10.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(10)
        End If
    End Sub
    Private Sub chkRegion11_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion11.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(11)
        End If
    End Sub
    Private Sub chkRegion22_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion22.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(22)
        End If
    End Sub
    Private Sub chkRegion21_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion21.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(21)
        End If
    End Sub
    Private Sub chkRegion20_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion20.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(20)
        End If
    End Sub
    Private Sub chkRegion19_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion19.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(19)
        End If
    End Sub
    Private Sub chkRegion18_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion18.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(18)
        End If
    End Sub
    Private Sub chkRegion17_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion17.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(17)
        End If
    End Sub
    Private Sub chkRegion16_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion16.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(16)
        End If
    End Sub
    Private Sub chkRegion15_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion15.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(15)
        End If
    End Sub
    Private Sub chkRegion14_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion14.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(14)
        End If
    End Sub
    Private Sub chkRegion13_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion13.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(13)
        End If
    End Sub
    Private Sub chkRegion12_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion12.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(12)
        End If
    End Sub
    Private Sub chkRegion44_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion44.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(44)
        End If
    End Sub
    Private Sub chkRegion43_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion43.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(43)
        End If
    End Sub
    Private Sub chkRegion42_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion42.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(42)
        End If
    End Sub
    Private Sub chkRegion41_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion41.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(41)
        End If
    End Sub
    Private Sub chkRegion40_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion40.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(40)
        End If
    End Sub
    Private Sub chkRegion39_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion39.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(39)
        End If
    End Sub
    Private Sub chkRegion38_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion38.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(38)
        End If
    End Sub
    Private Sub chkRegion37_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion37.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(37)
        End If
    End Sub
    Private Sub chkRegion36_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion36.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(36)
        End If
    End Sub
    Private Sub chkRegion35_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion35.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(35)
        End If
    End Sub
    Private Sub chkRegion34_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion34.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(34)
        End If
    End Sub
    Private Sub chkRegion33_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion33.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(33)
        End If
    End Sub
    Private Sub chkRegion32_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion32.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(32)
        End If
    End Sub
    Private Sub chkRegion31_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion31.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(31)
        End If
    End Sub
    Private Sub chkRegion30_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion30.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(30)
        End If
    End Sub
    Private Sub chkRegion29_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion29.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(29)
        End If
    End Sub
    Private Sub chkRegion28_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion28.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(28)
        End If
    End Sub
    Private Sub chkRegion27_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion27.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(27)
        End If
    End Sub
    Private Sub chkRegion26_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion26.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(26)
        End If
    End Sub
    Private Sub chkRegion25_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion25.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(25)
        End If
    End Sub
    Private Sub chkRegion24_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion24.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(24)
        End If
    End Sub
    Private Sub chkRegion23_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion23.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(23)
        End If
    End Sub
    Private Sub chkRegion67_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion67.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(67)
        End If
    End Sub
    Private Sub chkRegion66_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion66.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(66)
        End If
    End Sub
    Private Sub chkRegion65_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion65.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(65)
        End If
    End Sub
    Private Sub chkRegion64_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion64.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(64)
        End If
    End Sub
    Private Sub chkRegion63_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion63.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(63)
        End If
    End Sub
    Private Sub chkRegion62_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion62.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(62)
        End If
    End Sub
    Private Sub chkRegion61_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion61.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(61)
        End If
    End Sub
    Private Sub chkRegion60_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion60.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(60)
        End If
    End Sub
    Private Sub chkRegion59_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion59.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(59)
        End If
    End Sub
    Private Sub chkRegion58_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion58.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(58)
        End If
    End Sub
    Private Sub chkRegion57_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion57.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(57)
        End If
    End Sub
    Private Sub chkRegion56_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion56.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(56)
        End If
    End Sub
    Private Sub chkRegion55_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion55.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(55)
        End If
    End Sub
    Private Sub chkRegion54_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion54.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(54)
        End If
    End Sub
    Private Sub chkRegion53_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion53.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(53)
        End If
    End Sub
    Private Sub chkRegion52_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion52.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(52)
        End If
    End Sub
    Private Sub chkRegion51_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion51.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(51)
        End If
    End Sub
    Private Sub chkRegion50_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion50.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(50)
        End If
    End Sub
    Private Sub chkRegion49_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion49.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(49)
        End If
    End Sub
    Private Sub chkRegion48_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion48.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(48)
        End If
    End Sub
    Private Sub chkRegion47_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion47.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(47)
        End If
    End Sub
    Private Sub chkRegion46_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion46.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(46)
        End If
    End Sub
    Private Sub chkRegion45_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRegion45.CheckedChanged
        If InStr(sender.ToString, "CheckState: 1") <> 0 Then
            Call ClearSystemChecks()
            Call ClearAllRegionChecks(45)
        End If
    End Sub
#End Region

#End Region

    ' Initalizes all the prices tab boxes, etc
    Private Sub InitUpdatePricesTab()
        Dim i As Integer
        Dim TempRegion As String = ""

        FirstPriceChargeTypesComboLoad = True
        FirstPriceShipTypesComboLoad = True
        RefreshList = False

        Call ClearSystemChecks()
        Call ClearAllRegionChecks(0)

        txtPriceItemFilter.Text = ""

        With UserUpdatePricesTabSettings
            chkPriceRawMaterialPrices.Checked = .AllRawMats
            RunUpdatePriceList = False ' If the settings trigger an update, we don't want to update the prices
            chkMinerals.Checked = .Minerals
            chkIceProducts.Checked = .IceProducts
            chkGas.Checked = .Gas
            chkAbyssalMaterials.Checked = .AbyssalMaterials
            chkBPCs.Checked = .BPCs
            chkMisc.Checked = .Misc
            chkAncientRelics.Checked = .AncientRelics
            chkAncientSalvage.Checked = .AncientSalvage
            chkSalvage.Checked = .Salvage
            chkStructureRigs.Checked = .StationComponents
            chkStructureModules.Checked = .StructureModules
            chkPlanetary.Checked = .Planetary
            chkDatacores.Checked = .Datacores
            chkDecryptors.Checked = .Decryptors
            chkRawMats.Checked = .RawMats
            chkProcessedMats.Checked = .ProcessedMats
            chkAdvancedMats.Checked = .AdvancedMats
            chkMatsandCompounds.Checked = .MatsandCompounds
            chkDroneComponents.Checked = .DroneComponents
            chkBoosterMats.Checked = .BoosterMats
            chkPolymers.Checked = .Polymers
            chkAsteroids.Checked = .Asteroids
            chkPriceManufacturedPrices.Checked = .AllManufacturedItems
            RunUpdatePriceList = False ' If the settings trigger an update, we don't want to update the prices
            chkShips.Checked = .Ships
            chkModules.Checked = .Modules
            chkDrones.Checked = .Drones
            chkBoosters.Checked = .Boosters
            chkRigs.Checked = .Rigs
            chkCharges.Checked = .Charges
            chkSubsystems.Checked = .Subsystems
            chkStructures.Checked = .Structures
            chkTools.Checked = .Tools
            chkCapT2Components.Checked = .CapT2Components
            chkCapitalComponents.Checked = .CapitalComponents
            chkComponents.Checked = .Components
            chkHybrid.Checked = .Hybrid
            chkFuelBlocks.Checked = .FuelBlocks
            chkPricesT1.Checked = .T1
            chkPricesT2.Checked = .T2
            chkPricesT3.Checked = .T3
            chkPricesT4.Checked = .Storyline
            chkPricesT5.Checked = .Faction
            chkPricesT6.Checked = .Pirate
            chkImplants.Checked = .Implants
            chkCelestials.Checked = .Celestials
            chkDeployables.Checked = .Deployables
            cmbItemsSplitPrices.Text = .ItemsCombo
            cmbRawMatsSplitPrices.Text = .RawMatsCombo
            txtRawPriceModifier.Text = FormatPercent(.RawPriceModifier, 1)
            txtItemsPriceModifier.Text = FormatPercent(.ItemsPriceModifier, 1)
            If .UseESIData Then
                rbtnPriceSourceCCPData.Checked = True
            Else
                rbtnPriceSourceEVEMarketer.Checked = True
            End If
            If .UsePriceProfile Then
                rbtnPriceSettingPriceProfile.Checked = True
                pnlPriceProfiles.Visible = True
                pnlSinglePriceLocationSelect.Visible = False
                ' Disable other buttons and lists
                cmbRawMatsSplitPrices.Enabled = False
                lblRawMatsSplitPrices.Enabled = False
                cmbItemsSplitPrices.Enabled = False
                lblItemsSplitPrices.Enabled = False
                lblRawPriceModifier.Enabled = False
                lblItemsPriceModifier.Enabled = False
                txtRawPriceModifier.Enabled = False
                txtItemsPriceModifier.Enabled = False
            Else
                rbtnPriceSettingSingleSelect.Checked = True

                pnlPriceProfiles.Visible = False
                pnlSinglePriceLocationSelect.Visible = True
                ' Enable other buttons and lists
                cmbRawMatsSplitPrices.Enabled = True
                lblRawMatsSplitPrices.Enabled = True
                cmbItemsSplitPrices.Enabled = True
                lblItemsSplitPrices.Enabled = True
                lblRawPriceModifier.Enabled = True
                lblItemsPriceModifier.Enabled = True
                txtRawPriceModifier.Enabled = True
                txtItemsPriceModifier.Enabled = True
            End If

            ' Set the defaults for the default price profiles
            cmbRawMaterialsDefaultsPriceType.Text = .PPRawPriceType
            ' First load the regions combo, then set the default region
            DefaultPreviousRawRegion = .PPRawRegion
            Call LoadRegionCombo(cmbRawMaterialsDefaultsRegion, .PPRawRegion)
            ' Now that we have the default region, load up the systems based on that
            Call LoadPPDefaultsSystemCombo(cmbRawMaterialsDefaultsSystem, .PPRawRegion, .PPRawSystem)
            txtRawMaterialsDefaultsPriceMod.Text = FormatPercent(.PPRawPriceMod, 1)
            PPRawSystemsLoaded = True

            ' Set the defaults for the default price profiles
            cmbItemsDefaultsPriceType.Text = .PPItemsPriceType
            ' First load the regions combo, then set the default region
            DefaultPreviousItemsRegion = .PPItemsRegion
            Call LoadRegionCombo(cmbItemsDefaultsRegion, .PPRawRegion)
            ' Now that we have the default region, load up the systems based on that
            Call LoadPPDefaultsSystemCombo(cmbItemsDefaultsSystem, .PPItemsRegion, .PPItemsSystem)
            txtItemsDefaultsPriceMod.Text = FormatPercent(.PPItemsPriceMod, 1)
            PPItemsSystemsLoaded = True

        End With

        RunUpdatePriceList = True
        RefreshList = True

        ' Disable cancel
        btnCancelUpdate.Enabled = False

        ' Preload the systems combo
        Call LoadPriceSolarSystems()

        ' Set system/region 
        If UserUpdatePricesTabSettings.SelectedSystem <> "0" Then
            ' Check the preset systems fist
            Select Case UserUpdatePricesTabSettings.SelectedSystem
                Case "Jita"
                    chkSystems1.Checked = True
                Case "Amarr"
                    chkSystems2.Checked = True
                Case "Dodixie"
                    chkSystems3.Checked = True
                Case "Rens"
                    chkSystems4.Checked = True
                Case "Hek"
                    chkSystems5.Checked = True
                Case Else
                    cmbPriceSystems.Text = UserUpdatePricesTabSettings.SelectedSystem
            End Select

        Else ' They set a region
            ' Loop through the checks and check the ones they set
            IgnoreSystemCheckUpdates = True
            For i = 1 To RegionCheckBoxes.Count - 1
                If UserUpdatePricesTabSettings.SelectedRegions.Contains(RegionCheckBoxes(i).Text) Then
                    RegionCheckBoxes(i).Checked = True
                End If
            Next
            IgnoreSystemCheckUpdates = False
        End If

        UpdatePricesColumnClicked = UserUpdatePricesTabSettings.ColumnSort
        If UserUpdatePricesTabSettings.ColumnSortType = "Ascending" Then
            UpdatePricesColumnSortType = SortOrder.Ascending
        Else
            UpdatePricesColumnSortType = SortOrder.Descending
        End If

        ' Load up the price profile grids
        Call LoadPriceProfileGrids()

        ' Refresh the prices
        Call UpdatePriceList()

    End Sub

    ' Structure for loading price profiles in the appropriate grids
    Private Structure PriceProfile
        Dim GroupName As String
        Dim PriceType As String
        Dim RegionName As String
        Dim SolarSystemName As String
        Dim PriceModifier As Double
        Dim RawMaterial As Boolean
    End Structure

    ' Loads the price profiles system combo
    Private Sub LoadPPDefaultsSystemCombo(ByRef SystemCombo As ComboBox, ByVal Region As String, ByVal System As String)

        Dim SQL As String = ""
        Dim rsData As SQLiteDataReader

        SQL = "SELECT solarSystemName FROM SOLAR_SYSTEMS, REGIONS "
        SQL = SQL & "WHERE SOLAR_SYSTEMS.regionID = REGIONS.regionID "
        SQL = SQL & "AND REGIONS.regionName = '" & Region & "'"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsData = DBCommand.ExecuteReader
        SystemCombo.BeginUpdate()
        SystemCombo.Items.Clear()
        ' Add the all systems item
        SystemCombo.Items.Add(AllSystems)
        While rsData.Read
            SystemCombo.Items.Add(rsData.GetString(0))
        End While
        SystemCombo.Text = Region
        SystemCombo.EndUpdate()
        rsData.Close()
        SystemCombo.Text = System

    End Sub

    ' Loads up the settings in the price profile grids
    Private Sub LoadPriceProfileGrids()
        Dim rsPP As SQLiteDataReader
        Dim SQL As String
        Dim GroupRawFlagList As String = ""
        Dim Profiles As New List(Of PriceProfile)
        Dim TempProfile As PriceProfile

        SQL = "SELECT GROUP_NAME, PRICE_TYPE, REGION_NAME, SOLAR_SYSTEM_NAME, PRICE_MODIFIER, RAW_MATERIAL "
        SQL = SQL & "FROM PRICE_PROFILES WHERE ID = " & CStr(SelectedCharacter.ID)
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsPP = DBCommand.ExecuteReader

        While rsPP.Read
            ' Build the list of groups we have, then use to query the ones we don't
            GroupRawFlagList = GroupRawFlagList & "AND NOT (GROUP_NAME ='" & rsPP.GetString(0) & "' AND RAW_MATERIAL =" & CStr(rsPP.GetInt32(5)) & ") "

            TempProfile.GroupName = rsPP.GetString(0)
            TempProfile.PriceType = rsPP.GetString(1)
            TempProfile.RegionName = rsPP.GetString(2)
            TempProfile.SolarSystemName = rsPP.GetString(3)
            TempProfile.PriceModifier = rsPP.GetDouble(4)
            TempProfile.RawMaterial = CBool(rsPP.GetInt32(5))
            Profiles.Add(TempProfile)
        End While

        rsPP.Close()

        ' Now get everything we don't have
        SQL = "SELECT GROUP_NAME, PRICE_TYPE, REGION_NAME, SOLAR_SYSTEM_NAME, PRICE_MODIFIER, RAW_MATERIAL "
        SQL = SQL & "FROM PRICE_PROFILES WHERE ID = 0 " & GroupRawFlagList & ""
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsPP = DBCommand.ExecuteReader

        While rsPP.Read
            TempProfile.GroupName = rsPP.GetString(0)
            TempProfile.PriceType = rsPP.GetString(1)
            TempProfile.RegionName = rsPP.GetString(2)
            TempProfile.SolarSystemName = rsPP.GetString(3)
            TempProfile.PriceModifier = rsPP.GetDouble(4)
            TempProfile.RawMaterial = CBool(rsPP.GetInt32(5))
            Profiles.Add(TempProfile)
        End While

        rsPP.Close()

        ' Load the lists
        Dim listRow As New ListViewItem
        lstRawPriceProfile.Items.Clear()
        lstManufacturedPriceProfile.Items.Clear()
        lstRawPriceProfile.BeginUpdate()
        lstManufacturedPriceProfile.BeginUpdate()

        For i = 0 To Profiles.Count - 1
            With Profiles(i)
                listRow = New ListViewItem(.GroupName)
                'The remaining columns are subitems  
                listRow.SubItems.Add(.PriceType)
                listRow.SubItems.Add(.RegionName)
                listRow.SubItems.Add(.SolarSystemName)
                listRow.SubItems.Add(FormatPercent(.PriceModifier, 1))

                If .RawMaterial Then
                    Call lstRawPriceProfile.Items.Add(listRow)
                Else
                    Call lstManufacturedPriceProfile.Items.Add(listRow)
                End If
            End With
        Next

        rsPP.Close()

        ' Sort by group name - don't enable column sorting here - use desc since the function flips it
        Call ListViewColumnSorter(0, CType(lstRawPriceProfile, ListView), 0, SortOrder.Descending)
        Call ListViewColumnSorter(0, CType(lstManufacturedPriceProfile, ListView), 0, SortOrder.Descending)

        lstRawPriceProfile.EndUpdate()
        lstManufacturedPriceProfile.EndUpdate()
    End Sub

    ' Save the settings
    Private Sub btnSaveUpdatePrices_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveUpdatePrices.Click
        Dim i As Integer
        Dim TempSettings As UpdatePriceTabSettings = Nothing
        Dim TempRegions As New List(Of String)

        Dim RegionChecked As Boolean = False
        Dim SystemChecked As Boolean = False
        Dim SearchSystem As String = ""

        ' Make sure they have at least one region checked first
        For i = 1 To RegionCheckBoxes.Length - 1
            If RegionCheckBoxes(i).Checked = True Then
                RegionChecked = True
                Exit For
            End If
        Next i

        ' Check systems too
        For i = 1 To SystemCheckBoxes.Length - 1
            If SystemCheckBoxes(i).Checked = True Then
                ' Save the checked system (can only be one)
                SearchSystem = SystemCheckBoxes(i).Text
                SystemChecked = True
                Exit For
            End If
        Next

        ' Finally check system combo
        If Not SystemChecked And cmbPriceSystems.Text <> DefaultSystemPriceCombo Then
            SystemChecked = True
            SearchSystem = cmbPriceSystems.Text
        End If

        If Not RegionChecked And Not SystemChecked Then
            MsgBox("Must Choose a Region or System", MsgBoxStyle.Exclamation, Me.Name)
            Exit Sub
        End If

        If Not ItemsSelected() Then
            MsgBox("Must Choose at least one Item type", MsgBoxStyle.Exclamation, Me.Name)
            Exit Sub
        End If

        TempSettings.ItemsCombo = cmbItemsSplitPrices.Text
        TempSettings.RawMatsCombo = cmbRawMatsSplitPrices.Text

        TempSettings.RawPriceModifier = CDbl(txtRawPriceModifier.Text.Replace("%", "")) / 100
        TempSettings.ItemsPriceModifier = CDbl(txtItemsPriceModifier.Text.Replace("%", "")) / 100

        ' Search for a set system first
        TempSettings.SelectedSystem = "0"
        If cmbPriceSystems.Text <> "Select System" Then
            TempSettings.SelectedSystem = cmbPriceSystems.Text
        Else
            For i = 1 To SystemCheckBoxes.Count - 1
                If SystemCheckBoxes(i).Checked Then
                    ' Save it
                    TempSettings.SelectedSystem = SystemCheckBoxes(i).Text
                    Exit For
                End If
            Next
        End If

        ' If no system found, then region
        If TempSettings.SelectedSystem = "0" Then
            ' Loop through the region checks and find checked regions
            For i = 1 To RegionCheckBoxes.Count - 1
                If RegionCheckBoxes(i).Checked = True Then
                    TempRegions.Add(RegionCheckBoxes(i).Text)
                End If
            Next
            TempSettings.SelectedRegions = TempRegions
        End If

        ' Raw items
        ' Manufactured Items
        With TempSettings
            .AllRawMats = chkPriceRawMaterialPrices.Checked
            .Minerals = chkMinerals.Checked
            .IceProducts = chkIceProducts.Checked
            .Gas = chkGas.Checked
            .AbyssalMaterials = chkAbyssalMaterials.Checked
            .BPCs = chkBPCs.Checked
            .Misc = chkMisc.Checked
            .AncientRelics = chkAncientRelics.Checked
            .AncientSalvage = chkAncientSalvage.Checked
            .Salvage = chkSalvage.Checked
            .StationComponents = chkStructureRigs.Checked
            .StructureModules = chkStructureModules.Checked
            .Planetary = chkPlanetary.Checked
            .Datacores = chkDatacores.Checked
            .Decryptors = chkDecryptors.Checked
            .RawMats = chkRawMats.Checked
            .ProcessedMats = chkProcessedMats.Checked
            .AdvancedMats = chkAdvancedMats.Checked
            .MatsandCompounds = chkMatsandCompounds.Checked
            .DroneComponents = chkDroneComponents.Checked
            .BoosterMats = chkBoosterMats.Checked
            .Polymers = chkPolymers.Checked
            .Asteroids = chkAsteroids.Checked
            .AllManufacturedItems = chkPriceManufacturedPrices.Checked
            .Ships = chkShips.Checked
            .Modules = chkModules.Checked
            .Drones = chkDrones.Checked
            .Boosters = chkBoosters.Checked
            .Rigs = chkRigs.Checked
            .Charges = chkCharges.Checked
            .Subsystems = chkSubsystems.Checked
            .Structures = chkStructures.Checked
            .Tools = chkTools.Checked
            .CapT2Components = chkCapT2Components.Checked
            .CapitalComponents = chkCapitalComponents.Checked
            .Components = chkComponents.Checked
            .Hybrid = chkHybrid.Checked
            .FuelBlocks = chkFuelBlocks.Checked
            .T1 = chkPricesT1.Checked
            .T2 = chkPricesT2.Checked
            .T3 = chkPricesT3.Checked
            .Storyline = chkPricesT4.Checked
            .Faction = chkPricesT5.Checked
            .Pirate = chkPricesT6.Checked
            .Implants = chkImplants.Checked
            .Deployables = chkDeployables.Checked
            .Celestials = chkCelestials.Checked
            If rbtnPriceSourceCCPData.Checked Then
                .UseESIData = True
            Else
                .UseESIData = False
            End If
            If rbtnPriceSettingPriceProfile.Checked Then
                .UsePriceProfile = True
            Else
                .UsePriceProfile = False
            End If

            ' Price profile defaults
            .PPRawPriceType = cmbRawMaterialsDefaultsPriceType.Text
            .PPRawRegion = cmbRawMaterialsDefaultsRegion.Text
            .PPRawSystem = cmbRawMaterialsDefaultsSystem.Text
            .PPRawPriceMod = CDbl(txtRawMaterialsDefaultsPriceMod.Text.Replace("%", "")) / 100

            .PPItemsPriceType = cmbItemsDefaultsPriceType.Text
            .PPItemsRegion = cmbItemsDefaultsRegion.Text
            .PPItemsSystem = cmbItemsDefaultsSystem.Text
            .PPItemsPriceMod = CDbl(txtItemsDefaultsPriceMod.Text.Replace("%", "")) / 100
        End With

        TempSettings.ColumnSort = UpdatePricesColumnClicked

        If UpdatePricesColumnSortType = SortOrder.Ascending Then
            TempSettings.ColumnSortType = "Ascending"
        Else
            TempSettings.ColumnSortType = "Descending"
        End If

        ' Save the data in the XML file
        Call AllSettings.SaveUpdatePricesSettings(TempSettings)

        ' Save the data to the local variable
        UserUpdatePricesTabSettings = TempSettings

        MsgBox("Update Prices Settings Saved", vbInformation, Application.ProductName)
        btnDownloadPrices.Focus()
        Application.UseWaitCursor = False

    End Sub

    Private Sub lstPricesView_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles lstPricesView.MouseClick
        Call ListClicked(lstPricesView, sender, e)
    End Sub

    Private Sub lstRawPriceProfile_MouseClick(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles lstRawPriceProfile.MouseClick
        Call ListClicked(lstRawPriceProfile, sender, e)
    End Sub

    Private Sub lstManufacturedPriceProfile_MouseClick(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles lstManufacturedPriceProfile.MouseClick
        Call ListClicked(lstManufacturedPriceProfile, sender, e)
    End Sub

    ' Sets the price profile defaults for anything with ID = 0
    Private Sub SetPriceProfileDefaults(PriceType As String, PriceRegion As String, PriceSystem As String, PriceMod As String, RawMat As Boolean)
        Dim SQL As String = ""

        SQL = "UPDATE PRICE_PROFILES SET PRICE_TYPE = '" & Trim(PriceType) & "', REGION_NAME = '" & FormatDBString(PriceRegion) & "', "
        SQL = SQL & "SOLAR_SYSTEM_NAME = '" & FormatDBString(PriceSystem) & "', PRICE_MODIFIER = " & CStr(CDbl(PriceMod.Replace("%", "")) / 100) & " "
        SQL = SQL & "WHERE ID = 0 AND RAW_MATERIAL = "
        If RawMat Then
            SQL = SQL & "1"
        Else
            SQL = SQL & "0"
        End If

        EVEDB.ExecuteNonQuerySQL(SQL)

    End Sub

    ' Checks the user entry and then sends the type ids and regions to the cache update
    Private Sub btnImportPrices_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDownloadPrices.Click
        Dim i As Integer
        Dim j As Integer

        Dim RegionChecked As Boolean
        Dim SystemChecked As Boolean
        Dim readerSystems As SQLiteDataReader
        Dim SQL As String

        Dim RegionName As String = ""
        Dim Items As New List(Of PriceItem)
        Dim TempItem As PriceItem
        Dim SearchRegion As String = ""
        Dim SearchSystem As String = ""
        Dim NumSystems As Integer = 0

        RegionChecked = False
        SystemChecked = False

        ' Progress Bar Init
        pnlProgressBar.Value = 0

        Dim RegionSelectedCount As Integer = 0

        ' Make sure they have at least one region checked first
        For i = 1 To RegionCheckBoxes.Length - 1
            If RegionCheckBoxes(i).Checked = True Then
                RegionChecked = True
                RegionSelectedCount += 1
            End If
        Next i

        ' Check systems too
        For i = 1 To SystemCheckBoxes.Length - 1
            If SystemCheckBoxes(i).Checked = True Then
                ' Save the checked system (can only be one)
                SearchSystem = SystemCheckBoxes(i).Text
                SystemChecked = True
                Exit For
            End If
        Next

        ' Finally check system combo
        If Not SystemChecked And cmbPriceSystems.Text <> DefaultSystemPriceCombo Then
            SystemChecked = True
            SearchSystem = cmbPriceSystems.Text
        End If

        If Not RegionChecked And Not SystemChecked Then
            MsgBox("Must Choose a Region or System", MsgBoxStyle.Exclamation, Me.Name)
            GoTo ExitSub
        End If

        If Trim(cmbPriceSystems.Text) = "" Or (Not cmbPriceSystems.Items.Contains(cmbPriceSystems.Text) And cmbPriceSystems.Text <> "Select System") Then
            MsgBox("Invalid Solar System Name", vbCritical, Application.ProductName)
            GoTo ExitSub
        End If

        If Not ItemsSelected() Then
            MsgBox("Must Choose at least one Item type", MsgBoxStyle.Exclamation, Me.Name)
            GoTo ExitSub
        End If

        If rbtnPriceSourceCCPData.Checked And RegionSelectedCount > 1 Then
            MsgBox("You cannot choose more than one region when downloading CCP Data", MsgBoxStyle.Exclamation, Me.Name)
            GoTo ExitSub
        End If

        ' Working
        Call DisableUpdatePricesTab(True)

        ' Enable cancel
        btnCancelUpdate.Enabled = True

        Me.Refresh()
        Me.Cursor = Cursors.WaitCursor
        pnlStatus.Text = "Initializing Query..."
        Application.DoEvents()

        ' Find the checked region
        If rbtnPriceSettingSingleSelect.Checked Then
            If RegionChecked Then
                For i = 1 To (RegionCheckBoxes.Length - 1)
                    If RegionCheckBoxes(i).Checked Then
                        Select Case i
                            Case 15, 26, 36, 50, 59 'These have () in description

                                ' Find the location of the ( and trim back from that
                                RegionName = RegionCheckBoxes(i).Text
                                j = InStr(1, RegionName, "(")

                                RegionName = RegionName.Substring(0, j - 2)

                            Case Else
                                RegionName = RegionCheckBoxes(i).Text
                        End Select

                        SearchRegion = RegionName
                        Exit For
                    End If
                Next

                ' Get the system list string
                SQL = "SELECT regionID FROM REGIONS "
                SQL = SQL & "WHERE regionName = '" & SearchRegion & "'"

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerSystems = DBCommand.ExecuteReader
                If readerSystems.Read Then
                    SearchRegion = CStr(readerSystems.GetValue(0))
                Else
                    MsgBox("Invalid Region Name", vbCritical, Application.ProductName)
                    GoTo ExitSub
                End If

                readerSystems.Close()
                readerSystems = Nothing
                DBCommand = Nothing

            ElseIf SystemChecked Then
                ' Get the system list string
                SQL = "SELECT solarSystemID, regionName FROM SOLAR_SYSTEMS, REGIONS "
                SQL = SQL & "WHERE REGIONS.regionID = SOLAR_SYSTEMS.regionID AND solarSystemName = '" & SearchSystem & "'"

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerSystems = DBCommand.ExecuteReader
                If readerSystems.Read Then
                    SearchSystem = CStr(readerSystems.GetValue(0))
                Else
                    MsgBox("Invalid Solar System Name", vbCritical, Application.ProductName)
                    GoTo ExitSub
                End If

                readerSystems.Close()
                readerSystems = Nothing
                DBCommand = Nothing
            End If
        End If

        ' Build the list of types we want to update and include the type, region/system
        For i = 0 To lstPricesView.Items.Count - 1

            ' Only include items that are in the market (Market ID not null in Inventory Types)
            If lstPricesView.Items(i).SubItems(5).Text <> "" Then
                TempItem = New PriceItem
                TempItem.TypeID = CLng(lstPricesView.Items(i).SubItems(0).Text)
                TempItem.GroupName = GetPriceGroupName(TempItem.TypeID)

                ' If the group name exists, then look it up
                If TempItem.GroupName <> "" Then
                    TempItem.Manufacture = CBool(lstPricesView.Items(i).SubItems(4).Text)
                    TempItem.RegionID = ""

                    If rbtnPriceSettingSingleSelect.Checked Then
                        TempItem.RegionID = SearchRegion
                        TempItem.SystemID = SearchSystem
                        If TempItem.Manufacture Then
                            TempItem.PriceType = cmbItemsSplitPrices.Text
                            TempItem.PriceModifier = CDbl(txtItemsPriceModifier.Text.Replace("%", "")) / 100
                        Else
                            TempItem.PriceType = cmbRawMatsSplitPrices.Text
                            TempItem.PriceModifier = CDbl(txtRawPriceModifier.Text.Replace("%", "")) / 100
                        End If
                    Else
                        ' Using price profiles, so look up all the data per group name
                        Dim rsPP As SQLiteDataReader
                        SQL = "SELECT PRICE_TYPE, regionID, SOLAR_SYSTEM_NAME, PRICE_MODIFIER FROM PRICE_PROFILES, REGIONS "
                        SQL = SQL & "WHERE REGIONS.regionName = PRICE_PROFILES.REGION_NAME "
                        SQL = SQL & "AND (ID = " & CStr(SelectedCharacter.ID) & " OR ID = 0) AND GROUP_NAME = '" & TempItem.GroupName & "' ORDER BY ID DESC"

                        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                        rsPP = DBCommand.ExecuteReader

                        If rsPP.Read Then
                            TempItem.PriceType = rsPP.GetString(0)
                            If rsPP.GetString(2) = AllSystems Then
                                ' Can only do one region for price profile
                                TempItem.RegionID = CStr(rsPP.GetInt64(1))
                                TempItem.SystemID = ""
                            Else
                                ' Look up the system name
                                TempItem.SystemID = CStr(GetSolarSystemID(rsPP.GetString(2)))
                            End If
                            TempItem.PriceModifier = rsPP.GetDouble(3)
                        Else
                            Application.DoEvents()
                        End If
                    End If

                    ' Add the item to the list if not there and it's not a blueprint (we don't want to query blueprints since it will return bpo price and we are using this for bpc
                    If Not Items.Contains(TempItem) And Not lstPricesView.Items(i).SubItems(1).Text.Contains("Blueprint") Then
                        Items.Add(TempItem)
                    End If
                End If
            End If
        Next

        ' Load the prices
        Call LoadPrices(Items)

UpdateProgramPrices:

        ' Update all the prices in the program
        Call UpdateProgramPrices()

ExitSub:

        Application.UseWaitCursor = False
        Application.DoEvents()
        ' Enable tab
        Call DisableUpdatePricesTab(False)

        ' Disable cancel
        btnCancelUpdate.Enabled = False

        Me.Refresh()
        Me.Cursor = Cursors.Default
        pnlProgressBar.Visible = False
        pnlStatus.Text = ""

    End Sub

    ' Loads prices from the cache into the ITEM_PRICES table based on the info selected on the main form
    Private Sub LoadPrices(ByVal SentItems As List(Of PriceItem))
        Dim readerPrices As SQLiteDataReader
        Dim SQL As String = ""
        Dim i As Integer
        Dim RegionList As String
        Dim SelectedPrice As Double
        Dim MP As New MarketPriceInterface(pnlProgressBar)

        Dim PriceType As String = "" ' Default

        If rbtnPriceSourceCCPData.Checked Then

            Dim Items As New List(Of TypeIDRegion)
            ' Loop through each item and set it's pair for query
            For i = 0 To SentItems.Count - 1
                Dim Temp As New TypeIDRegion
                Temp.TypeIDs.Add(CStr(SentItems(i).TypeID))

                Dim RegionID As String
                ' Look up regionID since we can only look up regions in ESI
                If SentItems(i).SystemID <> "" Then
                    DBCommand = New SQLiteCommand("SELECT regionID FROM SOLAR_SYSTEMS WHERE solarsystemID = '" & SentItems(i).SystemID & "'", EVEDB.DBREf)
                    readerPrices = DBCommand.ExecuteReader
                    readerPrices.Read()
                    RegionID = CStr(readerPrices.GetInt64(0))
                    readerPrices.Close()
                Else
                    ' for ESI, only one region per update
                    RegionID = SentItems(i).RegionID
                End If

                ' Set the region
                Temp.RegionString = RegionID

                DBCommand = Nothing

                Items.Add(Temp)
            Next

            pnlStatus.Text = "Downloading prices..."

            ' Update the ESI prices cache
            If Not MP.UpdateMarketOrders(Items) Then
                ' Update Failed, don't reload everything
                Call MsgBox("Some prices did not update. Please try again.", vbInformation, Application.ProductName)
                pnlStatus.Text = ""
                Exit Sub
            End If
            pnlStatus.Text = ""

        Else
            ' First update the EVE Marketer cache
            If Not UpdatePricesCache(SentItems) Then
                ' Update Failed, don't reload everything
                Exit Sub
            End If

        End If

        ' Working
        pnlStatus.Text = "Updating Item Prices..."
        RegionList = ""
        pnlProgressBar.Value = 0
        pnlProgressBar.Minimum = 0
        pnlProgressBar.Maximum = SentItems.Count + 1
        pnlProgressBar.Visible = True

        Application.DoEvents()

        Call EVEDB.BeginSQLiteTransaction()

        ' Select the prices from the cache table
        For i = 0 To SentItems.Count - 1
            ' Use combo values for min or max.
            Select Case SentItems(i).PriceType
                Case "Min Sell"
                    PriceType = "sellMin"
                Case "Max Sell"
                    PriceType = "sellMax"
                Case "Avg Sell"
                    PriceType = "sellAvg"
                Case "Median Sell"
                    PriceType = "sellMedian"
                Case "Percentile Sell"
                    PriceType = "sellPercentile"
                Case "Min Buy"
                    PriceType = "buyMin"
                Case "Max Buy"
                    PriceType = "buyMax"
                Case "Avg Buy"
                    PriceType = "buyAvg"
                Case "Median Buy"
                    PriceType = "buyMedian"
                Case "Percentile Buy"
                    PriceType = "buyPercentile"
            End Select

            ' Build the region list for each item
            RegionList = ""
            If SentItems(i).SystemID = "" Then
                RegionList = SentItems(i).RegionID
            Else
                RegionList = SentItems(i).SystemID
            End If

            If rbtnPriceSourceEVEMarketer.Checked Then
                ' Load the data based on the option selected - regionlist contains a list of regions or the system we wanted to update
                SQL = "SELECT " & PriceType & " FROM ITEM_PRICES_CACHE WHERE TYPEID = " & CStr(SentItems(i).TypeID) & " AND RegionOrSystem = '" & RegionList & "' ORDER BY DateTime(UPDATEDATE) DESC"
            Else
                Dim LimittoBuy As Boolean = False
                Dim LimittoSell As Boolean = False
                Dim SystemID As String = ""
                Dim RegionID As String = ""

                If SentItems(i).SystemID <> "" Then
                    SystemID = RegionList
                Else
                    RegionID = RegionList
                End If

                ' Get the data from ESI so we need to do some calcuations depending on the type they want
                SQL = "SELECT "
                Select Case PriceType
                    Case "buyAvg"
                        SQL = SQL & "AVG(PRICE)"
                        LimittoBuy = True
                    Case "buyMax"
                        SQL = SQL & "MAX(PRICE)"
                        LimittoBuy = True
                    Case "buyMedian"
                        SQL = SQL & CalcMedian(SentItems(i).TypeID, RegionID, SystemID, True)
                    Case "buyMin"
                        SQL = SQL & "MIN(PRICE)"
                        LimittoBuy = True
                    Case "buyPercentile"
                        SQL = SQL & CalcPercentile(SentItems(i).TypeID, RegionID, SystemID, True)
                    Case "sellAvg"
                        SQL = SQL & "AVG(PRICE)"
                        LimittoSell = True
                    Case "sellMax"
                        SQL = SQL & "MAX(PRICE)"
                        LimittoSell = True
                    Case "sellMedian"
                        SQL = SQL & CalcMedian(SentItems(i).TypeID, RegionID, SystemID, False)
                    Case "sellMin"
                        SQL = SQL & "MIN(PRICE)"
                        LimittoSell = True
                    Case "sellPercentile"
                        SQL = SQL & CalcPercentile(SentItems(i).TypeID, RegionID, SystemID, False)
                End Select

                ' Set the main from etc
                SQL = SQL & " FROM MARKET_ORDERS WHERE TYPE_ID = " & CStr(SentItems(i).TypeID) & " "
                ' If they want a system, then limit all the data to that system id
                If SentItems(i).SystemID <> "" Then
                    SQL = SQL & "AND SOLAR_SYSTEM_ID = " & RegionList & " "
                Else
                    ' Use the region
                    SQL = SQL & "AND REGION_ID = " & RegionList & " "
                End If

                ' See if we limit to buy/sell only
                If LimittoBuy Then
                    SQL = SQL & "AND IS_BUY_ORDER <> 0"
                ElseIf LimittoSell Then
                    SQL = SQL & "AND IS_BUY_ORDER = 0"
                End If
            End If

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerPrices = DBCommand.ExecuteReader

            ' Grab the first record, which will be the latest one, if no record just leave what is already in item prices
            If readerPrices.Read Then
                If Not IsDBNull(readerPrices.GetValue(0)) Then
                    ' Modify the price depending on modifier
                    SelectedPrice = readerPrices.GetDouble(0) * (1 + SentItems(i).PriceModifier)

                    ' Now Update the ITEM_PRICES table, set price and price type
                    SQL = "UPDATE ITEM_PRICES SET PRICE = " & CStr(SelectedPrice) & ", PRICE_TYPE = '" & PriceType & "' WHERE ITEM_ID = " & CStr(SentItems(i).TypeID)
                    Call EVEDB.ExecuteNonQuerySQL(SQL)
                End If
                readerPrices.Close()
                readerPrices = Nothing
                DBCommand = Nothing
            End If

            ' For each record, update the progress bar
            Call IncrementToolStripProgressBar(pnlProgressBar)

            Application.DoEvents()
        Next

        Call EVEDB.CommitSQLiteTransaction()

        ' Done updating, hide the progress bar
        pnlProgressBar.Visible = False
        pnlStatus.Text = ""
        Application.DoEvents()

    End Sub

    ' Queries market orders and calculates the median and returns the median as a string
    Private Function CalcMedian(TypeID As Long, RegionID As String, SystemID As String, IsBuyOrder As Boolean) As String
        Dim MedianList As List(Of Double) = GetMarketOrderPriceList(TypeID, RegionID, SystemID, IsBuyOrder)
        Dim value As Double
        Dim size As Integer = MedianList.Count

        ' Calculate the median
        If size > 0 Then
            If size Mod 2 = 0 Then
                ' Need to average
                Dim a As Double = MedianList(CInt(size / 2 - 1))
                Dim b As Double = MedianList(CInt(size / 2))
                value = (a + b) / 2
            Else
                value = MedianList(CInt(Math.Floor(size / 2)))
            End If
        Else
            value = 0
        End If

        ' return 2 decimals
        Return FormatNumber(value, 2)

    End Function

    ' Queries market orders and calculates the percential price
    Private Function CalcPercentile(TypeID As Long, RegionID As String, SystemID As String, IsBuyOrder As Boolean) As String
        Dim PriceList As List(Of Double) = GetMarketOrderPriceList(TypeID, RegionID, SystemID, IsBuyOrder)
        Dim index As Integer

        If PriceList.Count > 0 Then
            If IsBuyOrder Then
                ' Get the top 5% 
                index = CInt(Math.Floor(0.95 * PriceList.Count))
            Else
                ' Get the bottom 5% for SELL or ALL - matches EVE Central?
                index = CInt(Math.Floor(0.05 * PriceList.Count))
            End If
            Return CStr(PriceList(index))
        Else
            Return "0.00"
        End If

    End Function

    ' Returns the list of prices for variables sent, sorted ascending
    Private Function GetMarketOrderPriceList(TypeID As Long, RegionID As String, SystemID As String, IsBuyOrder As Boolean) As List(Of Double)
        Dim SQL As String = ""
        Dim rsData As SQLiteDataReader
        Dim PriceList As New List(Of Double)

        SQL = "SELECT PRICE FROM MARKET_ORDERS WHERE TYPE_ID = " & CStr(TypeID) & " "
        If SystemID <> "" Then
            SQL = SQL & "AND SOLAR_SYSTEM_ID = " & SystemID & " "
        Else
            ' Use the region
            SQL = SQL & "AND REGION_ID = " & RegionID & " "
        End If

        If IsBuyOrder Then
            SQL = SQL & "AND IS_BUY_ORDER <> 0 "
        Else
            SQL = SQL & "AND IS_BUY_ORDER = 0 "
        End If

        SQL = SQL & "ORDER BY PRICE ASC"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsData = DBCommand.ExecuteReader

        While rsData.Read
            PriceList.Add(rsData.GetDouble(0))
        End While

        Return PriceList

    End Function

    ' Gets the group name from ITEM_PRICES
    Private Function GetPriceGroupName(TypeID As Long) As String
        Dim SQL As String = "SELECT ITEM_GROUP, ITEM_CATEGORY, ITEM_NAME FROM ITEM_PRICES WHERE ITEM_ID = " & CStr(TypeID)
        Dim rsGroup As SQLiteDataReader
        Dim RGN As String = ""
        Dim GN As String = ""
        Dim CN As String = ""
        Dim ITN As String = ""

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsGroup = DBCommand.ExecuteReader

        If rsGroup.Read Then
            GN = rsGroup.GetString(0)
            CN = rsGroup.GetString(1)
            ITN = rsGroup.GetString(2)

            Select Case GN
                Case "Mineral"
                    RGN = "Minerals"
                Case "Ice Product"
                    RGN = "Ice Products"
                Case "Datacores"
                    RGN = "Datacores"
                Case "Harvestable Cloud"
                    RGN = "Gas"
                Case "Abyssal Materials"
                    RGN = "Abyssal Materials"
                Case "Salvaged Materials"
                    RGN = "Salvage"
                Case "Ancient Salvage"
                    RGN = "Ancient Salvage"
                Case "Hybrid Polymers"
                    RGN = "Hybrid Polymers"
                Case "Moon Materials"
                    RGN = "Raw Moon Materials"
                Case "Intermediate Materials"
                    RGN = "Processed Moon Materials"
                Case "Composite"
                    RGN = "Advanced Moon Materials"
                Case "Materials and Compounds", "Artifacts and Prototypes", "Named Components"
                    RGN = "Materials & Compounds"
                Case "Biochemical Material"
                    RGN = "Booster Materials"
                Case "Advanced Capital Construction Components"
                    RGN = "Adv. Capital Construction Components"
                Case "Capital Construction Components"
                    RGN = "Capital Construction Components"
                Case "Construction Components"
                    RGN = "Construction Components"
                Case "Hybrid Tech Components"
                    RGN = "Hybrid Tech Components"
                Case "Tool"
                    RGN = "Tools"
                Case "Fuel Block"
                    RGN = "Fuel Blocks"
                Case "Station Components"
                    RGN = "Station Parts"
                Case "Booster"
                    RGN = "Boosters"
                Case Else
                    ' Do if checks or select on category
                    If GN.Contains("Decryptor") Then
                        RGN = "Decryptors"
                    ElseIf (GN = "General" Or GN = "Livestock" Or GN = "Radioactive" Or GN = "Biohazard" Or GN = "Commodities" _
                        Or GN = "Empire Insignia Drops" Or GN = "Criminal Tags" Or GN = "Miscellaneous" Or GN = "Unknown Components" Or GN = "Lease") _
                        And (ITN <> "Oxygen" And ITN <> "Water" And ITN <> "Elite Drone AI") Then
                        RGN = "Misc."
                    ElseIf GN = "Rogue Drone Components" Or ITN = "Elite Drone AI" Then
                        RGN = "Rogue Drone Components"
                    ElseIf GN = "Cyberimplant" Or (CN = "Implant" And GN <> "Booster") Then
                        RGN = "Implants"
                    ElseIf CN.Contains("Planetary") Or ITN = "Oxygen" Or ITN = "Water" Then
                        RGN = "Planetary"
                    ElseIf CN = "Blueprint" Then
                        RGN = "Blueprints"
                    ElseIf CN = "Ancient Relics" Then
                        RGN = "Ancient Relics"
                    ElseIf CN = "Deployable" Then
                        RGN = "Deployables"
                    ElseIf CN = "Asteroid" Then
                        RGN = "Asteroids"
                    ElseIf CN = "Ship" Then
                        RGN = "Ships"
                    ElseIf CN = "Subsystem" Then
                        RGN = "Subsystems"
                    ElseIf CN = "Structure Module" Then
                        RGN = "Structure Modules"
                    ElseIf CN = "Starbase" Then
                        RGN = "Structures"
                    ElseIf CN = "Charge" Then
                        RGN = "Charges"
                    ElseIf CN = "Drone" Or CN = "Fighter" Then
                        RGN = "Drones"
                    ElseIf CN = "Module" And Not GN.Contains("Rig") Then
                        RGN = "Modules"
                    ElseIf CN = "Module" And GN.Contains("Rig") Then
                        RGN = "Rigs"
                    ElseIf (CN = "Celestial" Or CN = "Orbitals" Or CN = "Sovereignty Structures" Or CN = "Station" Or CN = "Accessories" Or CN = "Infrastructure Upgrades") And GN <> "Harvestable Clound" Then
                        RGN = "Celestials"
                    ElseIf CN = "Structure" Then
                        RGN = "Structures"
                    ElseIf CN = "Structure Rigs" Then
                        RGN = "Structure Rigs"
                    Else
                        RGN = CN
                    End If
            End Select
        End If

        Return RGN

    End Function

    ' Adds prices for each type id and region to the cache by using the (my) EVE Central API Wrapper Class. 
    Private Function UpdatePricesCache(ByVal CacheItems As List(Of PriceItem)) As Boolean
        Dim TypeIDUpdatePriceList As New List(Of Long)
        Dim i As Integer
        Dim SQL As String = ""
        Dim PriceRecords As List(Of EVEMarketerPrice)
        Dim EVEMarketerPrices = New EVEMarketer
        Dim EVEMarketerError As MyError

        Dim RegionSystem As String = "" ' Used for querying the Price Cache for regions
        Dim RegionID As Integer = 0
        Dim SystemID As Integer = 0
        Dim TotalUpdateItems As Integer = 0 ' For progress bar, only count the ones we update
        Dim InsertRecord As Boolean = False
        Dim QueryEVEMarketer As Boolean = False
        Dim readerPriceCheck As SQLiteDataReader

        ' Reset the value of the progress bar
        pnlProgressBar.Value = 0
        If CacheItems.Count <> 0 Then
            pnlProgressBar.Maximum = CacheItems.Count - 1
        Else
            pnlProgressBar.Maximum = 0
        End If

        pnlProgressBar.Visible = True

        pnlStatus.Text = "Checking Items..."
        Application.DoEvents()

        ' Loop through the list of items to get full query of just those that need to be updated
        For i = 0 To CacheItems.Count - 1

            If CancelUpdatePrices Then
                Exit For
            End If

            ' Reset Insert
            InsertRecord = False

            ' Get the region/system list since they will always be the same, use the first one for EVE Central
            If CacheItems(i).SystemID <> "" Then
                RegionSystem = CacheItems(i).SystemID
                SystemID = CInt(RegionSystem)
            Else
                RegionSystem = CacheItems(i).RegionID
                RegionID = CInt(RegionSystem)
            End If

            ' See if the record is in the cache first
            SQL = "SELECT * FROM ITEM_PRICES_CACHE WHERE TYPEID = " & CStr(CacheItems(i).TypeID) & " And RegionOrSystem = '" & RegionSystem & "'"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerPriceCheck = DBCommand.ExecuteReader

            If Not readerPriceCheck.HasRows Then
                ' Not found
                InsertRecord = True
                readerPriceCheck.Close()
                readerPriceCheck = Nothing
                DBCommand = Nothing
            Else
                readerPriceCheck.Close()
                readerPriceCheck = Nothing
                DBCommand = Nothing

                ' There is a record, see if it needs to be updated (only update every 6 hours)
                SQL = "SELECT UPDATEDATE FROM ITEM_PRICES_CACHE WHERE TYPEID = " & CStr(CacheItems(i).TypeID) & " AND RegionOrSystem = '" & RegionSystem & "'"
                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerPriceCheck = DBCommand.ExecuteReader

                ' If no record or the max date
                If readerPriceCheck.Read Then
                    ' If older than the interval, add a new record
                    If DateTime.ParseExact(readerPriceCheck.GetString(0), SQLiteDateFormat, LocalCulture) < DateAdd(DateInterval.Hour, -1 * UserApplicationSettings.EVEMarketerRefreshInterval, Now) Then
                        InsertRecord = True
                    End If
                End If

                readerPriceCheck.Close()
                readerPriceCheck = Nothing
                DBCommand = Nothing

            End If

            ' Add to query item list for EVE Central
            If InsertRecord Then
                ' Add to the list
                TypeIDUpdatePriceList.Add(CacheItems(i).TypeID)

                ' Count up the update items
                TotalUpdateItems = TotalUpdateItems + 1
                ' We are inserting at least one record, so query eve central
                QueryEVEMarketer = True

            End If

            ' For each record, update the progress bar
            Call IncrementToolStripProgressBar(pnlProgressBar)

            Application.DoEvents()
        Next

        ' Don't show until download is done
        pnlProgressBar.Visible = False
        ' Reset the value of the progress bar
        pnlProgressBar.Value = 0
        ' Set the maximum updates for the progress bar
        pnlProgressBar.Maximum = TotalUpdateItems + 1

        If QueryEVEMarketer Then
            pnlStatus.Text = "Downloading Item Prices..."
            Application.DoEvents()

            ' Get the list of records to insert
            PriceRecords = EVEMarketerPrices.GetPrices(TypeIDUpdatePriceList, RegionID, SystemID)

            If IsNothing(PriceRecords) Then
                ' There was an error in the request 
                EVEMarketerError = EVEMarketerPrices.GetErrorData
                MsgBox("EVE Marketer Server is Unavailable" & Chr(13) & EVEMarketerError.Description & Chr(13) & "Please try again later", vbExclamation, Me.Text)
                UpdatePricesCache = False
                Exit Function
            End If

            ' Show the progress bar now and update status
            pnlProgressBar.Visible = True
            pnlStatus.Text = "Updating Price Cache..."
            Application.DoEvents()

            Call EVEDB.BeginSQLiteTransaction()

            ' Loop through the price records and insert each one
            For i = 0 To PriceRecords.Count - 1

                If CancelUpdatePrices Then
                    Exit For
                End If

                ' Insert record in Cache
                With PriceRecords(i)
                    ' First, delete the record
                    SQL = "DELETE FROM ITEM_PRICES_CACHE WHERE TYPEID = " & CStr(.TypeID) & " AND RegionOrSystem = '" & .RegionOrSystem & "'"
                    Call EVEDB.ExecuteNonQuerySQL(SQL)

                    ' Insert new data
                    SQL = "INSERT INTO ITEM_PRICES_CACHE (typeID, buyVolume, buyAvg, buyweightedAvg, buyMax, buyMin, buyStdDev, buyMedian, buyPercentile, buyVariance, "
                    SQL = SQL & "sellVolume, sellAvg, sellweightedAvg, sellMax, sellMin, sellStdDev, sellMedian, sellPercentile, sellVariance, RegionOrSystem, UpdateDate) VALUES "
                    SQL = SQL & "(" & CStr(.TypeID) & "," & CStr(.BuyVolume) & "," & CStr(.BuyAvgPrice) & "," & CStr(.BuyWeightedAveragePrice) & "," & CStr(.BuyMaxPrice) & "," & CStr(.BuyMinPrice) & "," & CStr(.BuyStdDev) & "," & CStr(.BuyMedian) & "," & CStr(.BuyPercentile) & "," & CStr(.BuyVariance) & ","
                    SQL = SQL & CStr(.SellVolume) & "," & CStr(.SellAvgPrice) & "," & CStr(.SellWeightedAveragePrice) & "," & CStr(.SellMaxPrice) & "," & CStr(.SellMinPrice) & "," & CStr(.SellStdDev) & "," & CStr(.SellMedian) & "," & CStr(.SellPercentile) & "," & CStr(.SellVariance) & ","
                    SQL = SQL & "'" & .RegionOrSystem & "','" & Format(Now, SQLiteDateFormat) & "')"

                End With

                Call EVEDB.ExecuteNonQuerySQL(SQL)

                ' For each record, update the progress bar
                Call IncrementToolStripProgressBar(pnlProgressBar)

                Application.DoEvents()
            Next

            Call EVEDB.CommitSQLiteTransaction()

        End If

        ' Done updating, hide the progress bar
        CancelUpdatePrices = False
        pnlProgressBar.Visible = False
        UpdatePricesCache = True
        pnlStatus.Text = ""
        Application.DoEvents()

    End Function

    ' Function just queries the items table based on the item type selection then updates the list
    Public Sub UpdatePriceList()
        Dim readerMats As SQLiteDataReader
        Dim SQL As String
        Dim TechSQL As String = ""
        Dim TechChecked As Boolean = False
        Dim lstViewRow As ListViewItem
        Dim ItemChecked As Boolean = False

        ' See if we want to run the update
        ' This will happen in times of things like selecting all boxes
        If Not RunUpdatePriceList Then
            Exit Sub
        End If

        ' Working
        Me.Cursor = Cursors.WaitCursor
        pnlStatus.Text = "Refreshing List..."
        Application.DoEvents()

        ' Add the marketGroupID to the list for checks later
        SQL = "SELECT ITEM_ID, ITEM_NAME, ITEM_GROUP, PRICE, MANUFACTURE, marketGroupID, PRICE_TYPE FROM ITEM_PRICES, INVENTORY_TYPES"
        SQL = SQL & " WHERE ITEM_PRICES.ITEM_ID = INVENTORY_TYPES.typeID AND ("

        ' Raw materials - non-manufacturable
        If chkMinerals.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Mineral' OR "
            ItemChecked = True
        End If
        If chkIceProducts.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Ice Product' OR "
            ItemChecked = True
        End If
        If chkPlanetary.Checked Then
            SQL = SQL & "(ITEM_CATEGORY LIKE 'Planetary%' OR ITEM_NAME IN ('Oxygen','Water')) OR "
            ItemChecked = True
        End If
        If chkDatacores.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Datacores' OR "
            ItemChecked = True
        End If
        If chkDecryptors.Checked Then
            SQL = SQL & "ITEM_GROUP LIKE '%Decryptor%' OR " ' Storyline decryptors are category 'Commodity'
            ItemChecked = True
        End If
        If chkGas.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Harvestable Cloud' OR "
            ItemChecked = True
        End If
        'If chkAbyssalMaterials.Checked Then
        '    SQL = SQL & "ITEM_GROUP LIKE 'Abyssal%' OR "
        '    ItemChecked = True
        'End If
        If chkBPCs.Checked Then
            SQL = SQL & "ITEM_CATEGORY = 'Blueprint' OR "
            ItemChecked = True
        End If
        If chkMisc.Checked Then ' Commodities = Shattered Villard Wheel
            SQL = SQL & "(ITEM_GROUP IN ('General','Livestock','Abyssal Materials','Radioactive','Biohazard','Commodities','Empire Insignia Drops','Criminal Tags','Miscellaneous','Unknown Components','Lease') AND ITEM_NAME NOT IN ('Oxygen','Water', 'Elite Drone AI')) OR "
            ItemChecked = True
        End If
        If chkSalvage.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Salvaged Materials' OR "
            ItemChecked = True
        End If
        If chkAncientSalvage.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Ancient Salvage' OR "
            ItemChecked = True
        End If
        If chkAncientRelics.Checked Then
            SQL = SQL & "ITEM_CATEGORY = 'Ancient Relics' OR "
            ItemChecked = True
        End If
        If chkPolymers.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Hybrid Polymers' OR "
            ItemChecked = True
        End If
        If chkRawMats.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Moon Materials' OR "
            ItemChecked = True
        End If
        If chkProcessedMats.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Intermediate Materials' OR "
            ItemChecked = True
        End If
        If chkAdvancedMats.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Composite' OR "
            ItemChecked = True
        End If
        If chkMatsandCompounds.Checked Then
            SQL = SQL & "ITEM_GROUP IN ('Materials and Compounds', 'Artifacts and Prototypes', 'Named Components') OR "
            ItemChecked = True
        End If
        If chkDroneComponents.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Rogue Drone Components' OR ITEM_NAME = 'Elite Drone AI' OR "
            ItemChecked = True
        End If
        If chkBoosterMats.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Biochemical Material' OR "
            ItemChecked = True
        End If
        If chkAsteroids.Checked Then
            SQL = SQL & "ITEM_CATEGORY = 'Asteroid' OR "
            ItemChecked = True
        End If

        ' Other Manufacturables
        If chkCapT2Components.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Advanced Capital Construction Components' OR "
            ItemChecked = True
        End If
        If chkCapitalComponents.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Capital Construction Components' OR "
            ItemChecked = True
        End If
        If chkComponents.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Construction Components' OR "
            ItemChecked = True
        End If
        If chkHybrid.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Hybrid Tech Components' OR "
            ItemChecked = True
        End If
        If chkTools.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Tool' OR "
            ItemChecked = True
        End If
        If chkFuelBlocks.Checked Then
            SQL = SQL & "ITEM_GROUP = 'Fuel Block' OR "
            ItemChecked = True
        End If
        If chkStructureRigs.Checked Then
            SQL = SQL & "ITEM_CATEGORY = 'Structure Rigs' OR "
            ItemChecked = True
        End If
        If chkImplants.Checked Then
            SQL = SQL & "(ITEM_GROUP = 'Cyberimplant' OR (ITEM_CATEGORY = 'Implant' AND ITEM_GROUP <> 'Booster')) OR "
            ItemChecked = True
        End If
        If chkDeployables.Checked Then
            SQL = SQL & "ITEM_CATEGORY = 'Deployable' OR "
            ItemChecked = True
        End If
        If chkStructureModules.Checked Then
            SQL = SQL & "(ITEM_CATEGORY = 'Structure Module' AND ITEM_GROUP NOT LIKE '%Rig%') OR "
            ItemChecked = True
        End If
        If chkCelestials.Checked Then
            SQL = SQL & "(ITEM_CATEGORY IN ('Celestial','Orbitals','Sovereignty Structures', 'Station', 'Accessories', 'Infrastructure Upgrades')  AND ITEM_GROUP <> 'Harvestable Cloud') OR "
            ItemChecked = True
        End If

        ' Manufactured Items
        If chkShips.Checked Or chkModules.Checked Or chkDrones.Checked Or chkBoosters.Checked Or chkRigs.Checked Or chkSubsystems.Checked Or chkStructures.Checked Or chkCharges.Checked Or chkStructureRigs.Checked Then

            ' Make sure we have at least one tech checked that is enabled
            TechChecked = CheckTechChecks()

            If Not TechChecked And Not ItemChecked Then
                ' There isn't an item checked before this and these items all require tech, so exit
                ItemChecked = False
            Else
                ItemChecked = True
            End If

            ' If they choose a tech level, then build this part of the SQL query
            If TechChecked Then
                If PriceCheckT1Enabled Then
                    If chkPricesT1.Checked Then
                        ' Add to SQL query for tech level
                        TechSQL = TechSQL & "ITEM_TYPE = 1 OR "
                    End If
                End If

                If PriceCheckT2Enabled Then
                    If chkPricesT2.Checked Then
                        ' Add to SQL query for tech level
                        TechSQL = TechSQL & "ITEM_TYPE = 2 OR "
                    End If
                End If

                If PriceCheckT3Enabled Then
                    If chkPricesT3.Checked Then
                        ' Add to SQL query for tech level
                        TechSQL = TechSQL & "ITEM_TYPE = 14 OR "
                    End If
                End If

                ' Add the Pirate, Storyline, Navy search string
                ' Storyline
                If PriceCheckT4Enabled Then
                    If chkPricesT4.Checked Then
                        ' Add to SQL query for tech level
                        TechSQL = TechSQL & "ITEM_TYPE = 3 OR "
                    End If
                End If

                ' Navy
                If PriceCheckT5Enabled Then
                    If chkPricesT5.Checked Then
                        ' Add to SQL query for tech level
                        TechSQL = TechSQL & "ITEM_TYPE = 16 OR "
                    End If
                End If

                ' Pirate
                If PriceCheckT6Enabled Then
                    If chkPricesT6.Checked Then
                        ' Add to SQL query for tech level
                        TechSQL = TechSQL & "ITEM_TYPE = 15 OR "
                    End If
                End If

                ' Format TechSQL - Add on Meta codes - 21,22,23,24 are T3
                If TechSQL <> "" Then
                    TechSQL = "(" & TechSQL.Substring(0, TechSQL.Length - 3) & "OR ITEM_TYPE IN (21,22,23,24)) "
                End If

                ' Build Tech 1,2,3 Manufactured Items
                If chkCharges.Checked Then
                    SQL = SQL & "(ITEM_CATEGORY = 'Charge' AND " & TechSQL
                    If cmbPriceChargeTypes.Text <> "All Charge Types" Then
                        SQL = SQL & " AND ITEM_GROUP = '" & cmbPriceChargeTypes.Text & "'"
                    End If
                    SQL = SQL & ") OR "
                End If
                If chkDrones.Checked Then
                    SQL = SQL & "(ITEM_CATEGORY IN ('Drone', 'Fighter') AND " & TechSQL & ") OR "
                End If
                If chkModules.Checked Then ' Not rigs but Modules
                    SQL = SQL & "(ITEM_CATEGORY = 'Module' AND ITEM_GROUP NOT LIKE 'Rig%' AND " & TechSQL & ") OR "
                End If
                If chkShips.Checked Then
                    SQL = SQL & "(ITEM_CATEGORY = 'Ship' AND " & TechSQL
                    If cmbPriceShipTypes.Text <> "All Ship Types" Then
                        SQL = SQL & " AND ITEM_GROUP = '" & cmbPriceShipTypes.Text & "'"
                    End If
                    SQL = SQL & ") OR "
                End If
                If chkSubsystems.Checked Then
                    SQL = SQL & "(ITEM_CATEGORY = 'Subsystem' AND " & TechSQL & ") OR "
                End If
                If chkBoosters.Checked Then
                    SQL = SQL & "(ITEM_GROUP = 'Booster' AND " & TechSQL & ") OR "
                End If
                If chkRigs.Checked Then ' Rigs
                    SQL = SQL & "((ITEM_CATEGORY = 'Module' AND ITEM_GROUP LIKE 'Rig%' AND " & TechSQL & ") OR (ITEM_CATEGORY = 'Structure Module' AND ITEM_GROUP LIKE '%Rig%')) OR "
                End If
                If chkStructures.Checked Then
                    SQL = SQL & "((ITEM_CATEGORY IN ('Starbase','Structure') AND " & TechSQL & ") OR ITEM_GROUP = 'Station Components') OR "
                End If
            Else
                ' No tech level chosen, so just continue with other options and skip these that require a tech selection
            End If
        End If

        ' Leave function if no items checked
        If Not ItemChecked Then
            lstPricesView.Items.Clear()
        Else
            ' Take off last OR and add the final )
            SQL = SQL.Substring(0, SQL.Length - 4)
            SQL = SQL & ")"

            ' Search based on text
            If txtPriceItemFilter.Text <> "" Then
                SQL = SQL & "AND " & GetSearchText(txtPriceItemFilter.Text, "ITEM_NAME", "ITEM_GROUP")
            End If

            ' See if we want prices that are 0 only
            If chkUpdatePricesNoPrice.Checked Then
                SQL = SQL & " AND PRICE = 0 "
            End If

            SQL = SQL & " ORDER BY ITEM_GROUP, ITEM_CATEGORY, ITEM_NAME"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerMats = DBCommand.ExecuteReader

            ' Clear List
            lstPricesView.Items.Clear()
            ' Disable sorting because it will crawl after we update if there are too many records
            lstPricesView.ListViewItemSorter = Nothing
            lstPricesView.BeginUpdate()
            Me.Cursor = Cursors.WaitCursor

            ' Fill list
            While readerMats.Read
                'ITEM_ID, ITEM_NAME, ITEM_GROUP, PRICE, MANUFACTURE
                lstViewRow = New ListViewItem(CStr(readerMats.GetValue(0))) ' ID
                'The remaining columns are subitems  
                lstViewRow.SubItems.Add(CStr(readerMats.GetString(2))) ' Group
                lstViewRow.SubItems.Add(CStr(readerMats.GetString(1))) ' Name
                lstViewRow.SubItems.Add(FormatNumber(readerMats.GetDouble(3), 2))
                lstViewRow.SubItems.Add(CStr(readerMats.GetValue(4)))
                If IsDBNull(readerMats.GetValue(5)) Then
                    lstViewRow.SubItems.Add("")
                Else
                    lstViewRow.SubItems.Add(CStr(readerMats.GetInt64(5)))
                End If
                ' Price Type - look it up
                lstViewRow.SubItems.Add(CStr(readerMats.GetString(6)))

                Call lstPricesView.Items.Add(lstViewRow)
            End While

            readerMats.Close()
            readerMats = Nothing
            DBCommand = Nothing

            ' Now sort this
            Dim TempType As SortOrder
            If UpdatePricesColumnSortType = SortOrder.Ascending Then
                TempType = SortOrder.Descending
            Else
                TempType = SortOrder.Ascending
            End If
            Call ListViewColumnSorter(UpdatePricesColumnClicked, CType(lstPricesView, ListView), UpdatePricesColumnClicked, TempType)
            Me.Cursor = Cursors.Default
            lstPricesView.EndUpdate()
        End If

        ' Reset
        txtListEdit.Visible = False
        Me.Cursor = Cursors.Default
        Application.DoEvents()
        pnlStatus.Text = ""

    End Sub

    ' Makes sure a tech is enabled and checked for items that require tech based on saved values, not current due to disabling form
    Private Function CheckTechChecks() As Boolean

        If PriceCheckT1Enabled Then
            If TechCheckBoxes(1).Checked Then
                Return True
            End If
        End If

        If PriceCheckT2Enabled Then
            If TechCheckBoxes(2).Checked Then
                Return True
            End If
        End If

        If PriceCheckT3Enabled Then
            If TechCheckBoxes(3).Checked Then
                Return True
            End If
        End If

        If PriceCheckT4Enabled Then
            If TechCheckBoxes(4).Checked Then
                Return True
            End If
        End If

        If PriceCheckT5Enabled Then
            If TechCheckBoxes(5).Checked Then
                Return True
            End If
        End If

        If PriceCheckT6Enabled Then
            If TechCheckBoxes(6).Checked Then
                Return True
            End If
        End If

        Return False

    End Function

    ' Loads the solar systems into the combo for system prices
    Private Sub LoadPriceSolarSystems()
        Dim SQL As String
        Dim readerSS As SQLiteDataReader

        ' Load the select systems combobox with systems - no WH systems
        SQL = "SELECT solarSystemName FROM SOLAR_SYSTEMS, REGIONS AS R WHERE SOLAR_SYSTEMS.regionID = R.regionID "
        SQL = SQL & "AND R.regionName NOT LIKE '%-R%' "
        SQL = SQL & "OR solarSystemName = 'Thera' "
        SQL = SQL & "ORDER BY solarSystemName"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerSS = DBCommand.ExecuteReader
        cmbPriceSystems.Items.Clear()
        cmbPriceSystems.BeginUpdate()
        While readerSS.Read
            cmbPriceSystems.Items.Add(readerSS.GetString(0))
        End While
        cmbPriceSystems.EndUpdate()
        readerSS.Close()
        readerSS = Nothing
        DBCommand = Nothing

        cmbPriceSystems.Text = "Select System"

    End Sub

    Private Sub LoadPriceShipTypes()
        Dim SQL As String
        Dim readerShipType As SQLiteDataReader

        ' Load the select systems combobox with systems
        SQL = "SELECT groupName from inventory_types, inventory_groups, inventory_categories "
        SQL = SQL & "WHERE  inventory_types.groupID = inventory_groups.groupID "
        SQL = SQL & "AND inventory_groups.categoryID = inventory_categories.categoryID "
        SQL = SQL & "AND categoryname = 'Ship' AND groupName NOT IN ('Rookie ship','Prototype Exploration Ship') "
        SQL = SQL & "AND inventory_types.published <> 0 and inventory_groups.published <> 0 and inventory_categories.published <> 0 "
        SQL = SQL & "GROUP BY groupName "

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerShipType = DBCommand.ExecuteReader

        cmbPriceShipTypes.Items.Add("All Ship Types")

        While readerShipType.Read
            cmbPriceShipTypes.Items.Add(readerShipType.GetString(0))
        End While

        readerShipType.Close()
        readerShipType = Nothing
        DBCommand = Nothing

        cmbPriceShipTypes.Text = "All Ship Types"

    End Sub

    Private Sub LoadPriceChargeTypes()
        Dim SQL As String
        Dim readerChargeType As SQLiteDataReader

        ' Load the select systems combobox with systems
        SQL = "SELECT groupName from inventory_types, inventory_groups, inventory_categories "
        SQL = SQL & "WHERE  inventory_types.groupID = inventory_groups.groupID "
        SQL = SQL & "AND inventory_groups.categoryID = inventory_categories.categoryID "
        SQL = SQL & "AND categoryname = 'Charge' "
        SQL = SQL & "AND inventory_types.published <> 0 and inventory_groups.published <> 0 and inventory_categories.published <> 0 "
        SQL = SQL & "GROUP BY groupName "

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerChargeType = DBCommand.ExecuteReader

        cmbPriceChargeTypes.Items.Add("All Charge Types")

        While readerChargeType.Read
            cmbPriceChargeTypes.Items.Add(readerChargeType.GetString(0))
        End While

        readerChargeType.Close()
        readerChargeType = Nothing
        DBCommand = Nothing

        cmbPriceChargeTypes.Text = "All Charge Types"

    End Sub

    Private Sub btnSavePricestoFile_Click(sender As System.Object, e As System.EventArgs) Handles btnSavePricestoFile.Click
        Dim MyStream As StreamWriter
        Dim FileName As String
        Dim OutputText As String
        Dim Price As ListViewItem

        Dim Items As ListView.ListViewItemCollection
        Dim i As Integer = 0

        ' Show the dialog
        Dim ExportTypeString As String
        Dim Separator As String
        Dim FileHeader As String

        If UserApplicationSettings.DataExportFormat = CSVDataExport Then
            ' Save file name with date
            FileName = "Price List - " & Format(Now, "MMddyyyy") & ".csv"
            ExportTypeString = CSVDataExport
            Separator = ","
            FileHeader = PriceListHeaderCSV
            SaveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
        ElseIf UserApplicationSettings.DataExportFormat = SSVDataExport Then
            ' Save file name with date
            FileName = "Price List - " & Format(Now, "MMddyyyy") & ".ssv"
            ExportTypeString = SSVDataExport
            Separator = ";"
            FileHeader = PriceListHeaderSSV
            SaveFileDialog.Filter = "ssv files (*.ssv*)|*.ssv*|All files (*.*)|*.*"
        Else
            ' Save file name with date
            FileName = "Price List - " & Format(Now, "MMddyyyy") & ".txt"
            ExportTypeString = DefaultTextDataExport
            Separator = "|"
            FileHeader = PriceListHeaderTXT
            SaveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        End If

        SaveFileDialog.FilterIndex = 1
        SaveFileDialog.RestoreDirectory = True
        SaveFileDialog.FileName = FileName

        If SaveFileDialog.ShowDialog() = DialogResult.OK Then
            Try
                MyStream = File.CreateText(SaveFileDialog.FileName)

                If Not (MyStream Is Nothing) Then

                    ' Output the buy list first
                    Items = lstPricesView.Items

                    If Items.Count > 0 Then
                        Me.Cursor = Cursors.WaitCursor

                        Application.DoEvents()

                        OutputText = FileHeader
                        MyStream.Write(OutputText & Environment.NewLine)

                        For Each Price In Items
                            Application.DoEvents()
                            ' Build the output text -"Group,Item Name,Price,Price Type,Raw Material,Type ID"
                            OutputText = Price.SubItems(1).Text & Separator
                            OutputText = OutputText & Price.SubItems(2).Text & Separator
                            If ExportTypeString = SSVDataExport Then
                                ' Format to EU
                                OutputText = OutputText & ConvertUStoEUDecimal(Price.SubItems(3).Text) & Separator
                            Else
                                OutputText = OutputText & Format(Price.SubItems(3).Text, "Fixed") & Separator
                            End If
                            OutputText = OutputText & Price.SubItems(6).Text & Separator
                            ' Manufacturing flag - set if raw mat or not (raw mats are not manufactured)
                            If Price.SubItems(4).Text = "0" Then
                                OutputText = OutputText & "TRUE" & Separator
                            Else
                                OutputText = OutputText & "FALSE" & Separator
                            End If
                            OutputText = OutputText & Price.SubItems(0).Text

                            MyStream.Write(OutputText & Environment.NewLine)
                        Next

                    End If

                    MyStream.Flush()
                    MyStream.Close()

                    MsgBox("Price List Saved", vbInformation, Application.ProductName)

                End If
            Catch
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
            End Try
        End If

        ' Done processing 
        Me.Cursor = Cursors.Default
        Me.Refresh()
        Application.DoEvents()

    End Sub

    Private Sub btnLoadPricesfromFile_Click(sender As System.Object, e As System.EventArgs) Handles btnLoadPricesfromFile.Click
        Dim SQL As String
        Dim BPStream As StreamReader = Nothing
        Dim openFileDialog1 As New OpenFileDialog()
        Dim Line As String
        Dim ParsedLine As String()
        Dim Separator As String = ""
        Dim FileType As String

        If UserApplicationSettings.DataExportFormat = CSVDataExport Then
            FileType = CSVDataExport
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            openFileDialog1.FileName = "*.csv"
            openFileDialog1.FilterIndex = 2
            openFileDialog1.RestoreDirectory = True
        ElseIf UserApplicationSettings.DataExportFormat = SSVDataExport Then
            FileType = SSVDataExport
            openFileDialog1.Filter = "ssv files (*.ssv*)|*.ssv*|All files (*.*)|*.*"
            openFileDialog1.FileName = "*.ssv"
            openFileDialog1.FilterIndex = 2
            openFileDialog1.RestoreDirectory = True
        Else
            FileType = DefaultTextDataExport
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            openFileDialog1.FileName = "*.txt"
            openFileDialog1.FilterIndex = 2
            openFileDialog1.RestoreDirectory = True
        End If

        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                BPStream = New StreamReader(openFileDialog1.FileName)

                If (BPStream IsNot Nothing) Then
                    ' Read the file line by line here, start with headers
                    Line = BPStream.ReadLine
                    Line = BPStream.ReadLine ' First line of data

                    If Line Is Nothing Then
                        ' Leave loop
                        Exit Try
                    Else
                        ' disable the tab
                        Call DisableUpdatePricesTab(True)
                    End If

                    Call EVEDB.BeginSQLiteTransaction()
                    Application.UseWaitCursor = True

                    While Line IsNot Nothing
                        Application.DoEvents()
                        ' Format is: Group Name, Item Name, Price, Price Type, Raw Material, Type ID

                        ' Parse it
                        Select Case FileType
                            Case CSVDataExport
                                ParsedLine = Line.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
                            Case SSVDataExport
                                ParsedLine = Line.Split(New Char() {";"c}, StringSplitOptions.RemoveEmptyEntries)
                            Case Else
                                ParsedLine = Line.Split(New Char() {"|"c}, StringSplitOptions.RemoveEmptyEntries)
                        End Select

                        ' Loop through and update the price and price type, the rest is static
                        SQL = "UPDATE ITEM_PRICES SET "
                        If FileType = SSVDataExport Then
                            ' Need to swap periods and commas before inserting
                            ParsedLine(2) = ParsedLine(2).Replace(".", "") ' Just replace the periods as they are commas for numbers, which aren't needed
                            ParsedLine(2) = ParsedLine(2).Replace(",", ".") ' now update the commas for decimal
                        Else
                            ParsedLine(2) = ParsedLine(2).Replace(",", "") ' Make sure we format correctly, strip out any commas
                        End If
                        SQL = SQL & "PRICE = " & ParsedLine(2) & ","
                        SQL = SQL & "PRICE_TYPE = '" & ParsedLine(3) & "' "
                        SQL = SQL & "WHERE ITEM_ID = " & ParsedLine(5)

                        ' Update the record
                        Call EVEDB.ExecuteNonQuerySQL(SQL)

                        Line = BPStream.ReadLine ' Read next line

                    End While

                    Call EVEDB.CommitSQLiteTransaction()

                    Application.UseWaitCursor = False
                    MsgBox("Prices Loaded", vbInformation, Application.ProductName)

                End If
            Catch Ex As Exception
                Application.UseWaitCursor = False
                Call EVEDB.RollbackSQLiteTransaction()
                MessageBox.Show("Cannot read file from disk. Original error: " & Ex.Message)
            Finally
                ' Check this again, since we need to make sure we didn't throw an exception on open.
                If (BPStream IsNot Nothing) Then
                    BPStream.Close()
                End If
            End Try
        End If

        Application.UseWaitCursor = False
        ' Enable the tab
        Call DisableUpdatePricesTab(False)
        Call UpdatePriceList()
        Application.DoEvents()

    End Sub

#End Region

#Region "Manufacturing"

#Region "Manufacturing Object Functions"

    Private Sub lstManufacturing_KeyDown(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles lstManufacturing.KeyDown

        If e.KeyCode = Keys.C AndAlso e.Control = True Then ' Copy
            ' Find the bp record selected
            Dim FoundItem As New ManufacturingItem
            ' Find the item clicked in the list of items then just send those values over
            ManufacturingRecordIDToFind = CLng(lstManufacturing.SelectedItems(0).SubItems(0).Text)
            FoundItem = FinalManufacturingItemList.Find(AddressOf FindManufacturingItem)

            ' Copy the bp to the clipboard
            CopyTextToClipboard(FoundItem.Blueprint.GetName)
        End If

    End Sub

    Private Sub btnCalcShowAssets_Click(sender As System.Object, e As System.EventArgs) Handles btnCalcShowAssets.Click
        ' Make sure it's not disposed
        If IsNothing(frmDefaultAssets) Then
            ' Make new form
            frmDefaultAssets = New frmAssetsViewer(AssetWindow.ManufacturingTab)
        Else
            If frmDefaultAssets.IsDisposed Then
                ' Make new form
                frmDefaultAssets = New frmAssetsViewer(AssetWindow.ManufacturingTab)
            End If
        End If

        ' Now open the Asset List
        frmDefaultAssets.Show()
        frmDefaultAssets.Focus()

        Application.DoEvents()
    End Sub

    Private Sub cmbCalcFWManufUpgradeLevel_SelectedIndexChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcFWCopyUpgradeLevel_SelectedIndexChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcFWInventionUpgradeLevel_SelectedIndexChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcFWUpgrade_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs)
        e.Handled = True
    End Sub

    Private Sub CheckRelicCalcChecks()
        Dim i As Integer

        If chkCalcT3.Checked Then
            If Not FirstLoad Then
                For i = 1 To CalcRelicCheckboxes.Count - 1
                    If CalcRelicCheckboxes(i).Checked Then
                        Exit Sub
                    End If
                Next

                ' One wasn't checked, display message and check wrecked
                MsgBox("Must select at least one relic type", vbExclamation, Application.ProductName)
                chkCalcRERelic1.Checked = True
            End If
        End If

    End Sub

    Private Sub CheckDecryptorChecks()
        Dim i As Integer

        ' Only check decryptor checks if they have T2 bp's checked
        If chkCalcT2.Checked Then
            If Not FirstLoad Then
                For i = 1 To CalcDecryptorCheckBoxes.Count - 1
                    If CalcDecryptorCheckBoxes(i).Checked Then
                        GoTo CheckTechs
                    End If
                Next

                ' One wasn't checked, display message and check wrecked
                MsgBox("Must select at least one decryptor type", vbExclamation, Application.ProductName)
                chkCalcDecryptor1.Checked = True
            End If
        End If

CheckTechs:

        If chkCalcDecryptorforT2.Enabled And chkCalcDecryptorforT3.Enabled Then
            ' If both enabled, one needs to be checked
            If chkCalcDecryptorforT2.Checked = False And chkCalcDecryptorforT3.Checked = False Then
                MsgBox("Must select Decryptor if using Tech 2 and 3", vbExclamation, Application.ProductName)
                chkCalcDecryptorforT2.Checked = True
            End If
        ElseIf chkCalcDecryptorforT2.Enabled Then
            If chkCalcDecryptorforT2.Checked = False Then
                MsgBox("Must select Decryptor if using Tech 2", vbExclamation, Application.ProductName)
                chkCalcDecryptorforT2.Checked = True
            End If
        ElseIf chkCalcDecryptorforT3.Enabled Then
            If chkCalcDecryptorforT3.Checked = False Then
                MsgBox("Must select Decryptor if using Tech 3", vbExclamation, Application.ProductName)
                chkCalcDecryptorforT3.Checked = True
            End If
        End If

    End Sub

    Private Sub txtCalcProdLines_DoubleClick(sender As Object, e As System.EventArgs) Handles txtCalcProdLines.DoubleClick
        ' Enter the max lines we have
        txtCalcProdLines.Text = CStr(MaximumProductionLines)
    End Sub

    Private Sub txtCalcProdLines_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcProdLines.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcProdLines_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcProdLines.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcBPs_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcNumBPs.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcBPs_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcNumBPs.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcRuns_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcRuns.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcLabLines_DoubleClick(sender As Object, e As System.EventArgs) Handles txtCalcLabLines.DoubleClick
        ' Enter the max lab lines we have
        txtCalcLabLines.Text = CStr(MaximumLaboratoryLines)
    End Sub

    Private Sub txtCalcLabLines_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcLabLines.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcLabLines_TextChanged(sender As Object, e As System.EventArgs) Handles txtCalcLabLines.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreLowSVR_CheckedChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcAvgPriceDuration_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles cmbCalcAvgPriceDuration.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub cmbCalcAvgPriceDuration_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbCalcAvgPriceDuration.SelectedIndexChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreMinerals_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreT1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcTaxes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcTaxes.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcFees_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcFees.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcUsage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreRAMS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreInvention_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcEstimateCopyCost_CheckedChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreInvention_CheckedChanged_1(sender As System.Object, e As System.EventArgs) Handles chkCalcIgnoreInvention.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreMinerals_CheckedChanged_1(sender As System.Object, e As System.EventArgs) Handles chkCalcIgnoreMinerals.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIgnoreT1Item_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcIgnoreT1Item.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub rbtnCalcCompareAll_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnCalcCompareAll.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcSVRThreshold_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcSVRThreshold.KeyPress
        ' Only allow numbers, decimal, negative or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedDecimalChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcSVRThreshold_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcSVRThreshold.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcSVRIncludeNull_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcSVRIncludeNull.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub rbtnCalcCompareBuildBuy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnCalcCompareBuildBuy.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub rbtnCalcCompareRawMats_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnCalcCompareRawMats.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub rbtnCalcCompareComponents_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnCalcCompareComponents.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcPPU_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcPPU.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcShipBPCNoD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcShipBPCD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcNonShipBPCNoD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcNonShipBPCD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcCanBuild_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcCanBuild.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcCanInvent_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcCanInvent.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtTempME_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcTempME.KeyPress
        ' Only allow numbers, negative or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedMETEChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtTempPE_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcTempTE.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedMETEChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcItemFilter_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcItemFilter.TextChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcRERelic1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRERelic1.CheckedChanged
        Call CheckRelicCalcChecks()
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcUseMaxBPCRunsNoRunsDecryptor_CheckedChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRERelic2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRERelic2.CheckedChanged
        Call CheckRelicCalcChecks()
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRERelic3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRERelic3.CheckedChanged
        Call CheckRelicCalcChecks()
        Call ResetRefresh()
    End Sub

    Private Sub btnCalcReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCalcReset.Click
        FirstManufacturingGridLoad = True ' Reset
        Call InitManufacturingTab()
        ' Load the calc types because it won't get loaded if firstmanufacturinggridload = true
        Call LoadCalcBPTypes()
    End Sub

    Private Sub cmbCalcBPTypeFilter_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbCalcBPTypeFilter.DropDown
        If FirstLoadCalcBPTypes Then
            Call LoadCalcBPTypes()
            FirstLoadCalcBPTypes = False
        End If
    End Sub

    Private Sub cmbCalcBPTypeFilter_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbCalcBPTypeFilter.GotFocus
        Call cmbCalcBPTypeFilter.SelectAll()
    End Sub

    Private Sub cmbCalcBPTypeFilter_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles cmbCalcBPTypeFilter.KeyPress
        ' Only let them select a bp by clicking
        Dim i As Integer
        i = 0
    End Sub

    Private Sub cmbCalcBPTypeFilter_SelectedValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbCalcBPTypeFilter.SelectedValueChanged
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcBPTypeFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbCalcBPTypeFilter.Click
        Call cmbCalcBPTypeFilter.SelectAll()
    End Sub

    Private Sub chkCalcOnlyOwnedBPOInvent_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcT1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcT1.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcT2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcT2.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            EnableDisableT2T3Options()
        End If
    End Sub

    Private Sub chkCalcT3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcT3.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            Call EnableDisableT2T3Options()
        End If
    End Sub

    ' Enables and disables the checks on the screen when T2 or T3 is selected
    Private Sub EnableDisableT2T3Options()

        FirstLoadCalcBPTypes = True
        cmbCalcBPTypeFilter.Text = "All Types"
        Call ResetRefresh()

        ' If checked, show the options, disable if not
        If chkCalcT2.Checked = True Then
            chkCalcCanInvent.Enabled = True
            chkCalcCanInvent.Enabled = True
            chkCalcIncludeT2Owned.Enabled = True
            chkCalcDecryptorforT2.Enabled = True
        Else
            chkCalcCanInvent.Enabled = False
            chkCalcCanInvent.Enabled = False
            chkCalcIncludeT2Owned.Enabled = False
            chkCalcDecryptorforT2.Enabled = False
        End If

        ' If T3 checked, enable T3 options, else disable
        If chkCalcT3.Checked = True Then
            gbCalcRelics.Enabled = True
            chkCalcIncludeT3Owned.Enabled = True
            chkCalcDecryptorforT3.Enabled = True
        Else
            gbCalcRelics.Enabled = False
            chkCalcIncludeT3Owned.Enabled = False
            chkCalcDecryptorforT3.Enabled = False
        End If

        If chkCalcT3.Checked = False And chkCalcT2.Checked = False Then
            gbCalcInvention.Enabled = False
        Else
            gbCalcInvention.Enabled = True
        End If

        ' Auto check if only one option enabled
        If chkCalcDecryptorforT2.Enabled And chkCalcT2.Checked And chkCalcDecryptorforT3.Enabled = False Then
            ' Auto check this
            chkCalcDecryptorforT2.Checked = True
        ElseIf chkCalcDecryptorforT3.Enabled And chkCalcT3.Checked And chkCalcDecryptorforT2.Enabled = False Then
            ' Auto check this
            chkCalcDecryptorforT3.Checked = True
        End If

    End Sub

    Private Sub chkCalcT2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkCalcT2.Click
        Call CheckDecryptorChecks()
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcT3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkCalcT3.Click
        Call CheckRelicCalcChecks()
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcStoryline_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcStoryline.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcNavyFaction_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcNavyFaction.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcPirateFaction_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcPirateFaction.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcShips_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcShips.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcModules_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcModules.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDrones_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDrones.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcAmmo_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcAmmo.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcRigs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRigs.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcComponents_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcComponents.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcStationParts_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcStructureRigs.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcSubsystems_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcSubsystems.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcStructures_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcStructures.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcBoosters_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcBoosters.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcMisc_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcMisc.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcCelestials_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcCelestials.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcReactions_CheckedChanged(sender As Object, e As EventArgs) Handles chkCalcReactions.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcStructureModules_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcStructureModules.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDeployables_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcDeployables.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub rbtnCalcBPOwned_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnCalcBPOwned.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            gbCalcIncludeOwned.Enabled = True
            Call ResetRefresh()
        End If
    End Sub

    Private Sub rbtnCalcAllBPs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rbtnCalcAllBPs.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            gbCalcIncludeOwned.Enabled = False
            Call ResetRefresh()
        End If
    End Sub

    Private Sub rbtnCalcBPFavorites_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rbtnCalcBPFavorites.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            gbCalcIncludeOwned.Enabled = True
            Call ResetRefresh()
        End If
    End Sub

    Private Sub txtCalcItemFilter_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtCalcItemFilter.KeyDown
        'Call ProcessCutCopyPasteSelect(txtCalcItemFilter, e)
        If e.KeyCode = Keys.Enter Then
            Call ResetRefresh()
            Call DisplayManufacturingResults(False)
        End If
    End Sub

    Private Sub btnCalcResetTextSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCalcResetTextSearch.Click
        txtCalcItemFilter.Text = ""
        txtCalcItemFilter.Focus()
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub txtCalcBPCCosts_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub chkCalcDecryptor0_Click(sender As System.Object, e As System.EventArgs) Handles chkCalcDecryptor0.Click
        ' Change the name based on the check state
        If chkCalcDecryptor0.CheckState = CheckState.Unchecked Then
            chkCalcDecryptor0.Text = "Optimal"
        ElseIf chkCalcDecryptor0.CheckState = CheckState.Checked Then
            chkCalcDecryptor0.Text = "Optimal IPH"
        ElseIf chkCalcDecryptor0.CheckState = CheckState.Indeterminate Then
            chkCalcDecryptor0.Text = "Optimal Profit"
        End If

        If Not FirstLoad Then
            ' For this one, it's optimal so disable all the others if checked
            If chkCalcDecryptor0.Checked Then
                chkCalcDecryptor1.Enabled = False
                chkCalcDecryptor2.Enabled = False
                chkCalcDecryptor3.Enabled = False
                chkCalcDecryptor4.Enabled = False
                chkCalcDecryptor5.Enabled = False
                chkCalcDecryptor6.Enabled = False
                chkCalcDecryptor7.Enabled = False
                chkCalcDecryptor8.Enabled = False
                chkCalcDecryptor9.Enabled = False
            Else
                chkCalcDecryptor1.Enabled = True
                chkCalcDecryptor2.Enabled = True
                chkCalcDecryptor3.Enabled = True
                chkCalcDecryptor4.Enabled = True
                chkCalcDecryptor5.Enabled = True
                chkCalcDecryptor6.Enabled = True
                chkCalcDecryptor7.Enabled = True
                chkCalcDecryptor8.Enabled = True
                chkCalcDecryptor9.Enabled = True
            End If
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDecryptor1.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDecryptor2.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor3_CheckedChanged_1(sender As System.Object, e As System.EventArgs) Handles chkCalcDecryptor3.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDecryptor4.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDecryptor5.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDecryptor6.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor7_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcDecryptor7.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor8_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcDecryptor8.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptor9_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcDecryptor9.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptorforT2_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcDecryptorforT2.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcDecryptorforT3_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcDecryptorforT3.CheckedChanged
        If Not FirstLoad Then
            Call CheckDecryptorChecks()
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcIncludeT2Owned_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcIncludeT2Owned.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcIncludeT3Owned_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcIncludeT3Owned.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRaceAmarr_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRaceAmarr.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRaceCaldari_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRaceCaldari.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRaceGallente_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRaceGallente.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRaceMinmatar_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRaceMinmatar.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRacePirate_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRacePirate.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcRaceOther_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCalcRaceOther.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcHistoryRegion_DropDown(sender As Object, e As System.EventArgs) Handles cmbCalcHistoryRegion.DropDown
        If Not CalcHistoryRegionLoaded Then
            Call LoadRegionCombo(cmbCalcHistoryRegion, cmbCalcHistoryRegion.Text)
            CalcHistoryRegionLoaded = True
        End If
    End Sub

    Private Sub cmbCalcSVRRegion_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles cmbCalcHistoryRegion.KeyPress
        e.Handled = True
    End Sub

    Private Sub cmbCalcSVRRegion_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbCalcHistoryRegion.SelectedIndexChanged
        Call ResetRefresh()
    End Sub

    Private Sub cmbCalcBuildTimeMod_SelectedIndexChanged(sender As System.Object, e As System.EventArgs)
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcSmall_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcSmall.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcMedium_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcMedium.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcLarge_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcLarge.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcXL_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcXL.CheckedChanged
        If Not FirstManufacturingGridLoad Then
            FirstLoadCalcBPTypes = True
            cmbCalcBPTypeFilter.Text = "All Types"
            Call ResetRefresh()
        End If
    End Sub

    Private Sub btnCalcSelectColumns_Click(sender As System.Object, e As System.EventArgs) Handles btnCalcSelectColumns.Click
        Dim f1 As New frmSelectManufacturingTabColumns
        ManufacturingTabColumnsChanged = False
        f1.ShowDialog()

        ' Now Refresh the grid if it changed
        If ManufacturingTabColumnsChanged Then
            If lstManufacturing.Items.Count <> 0 Then
                RefreshCalcData = True
                Call DisplayManufacturingResults(False)
            Else
                Call RefreshManufacturingTabColumns()
            End If
        End If
    End Sub

    Private Sub txtCalcTempME_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcTempME.TextChanged
        Call VerifyMETEEntry(txtCalcTempME, "ME")
    End Sub

    Private Sub txtCalcTempTE_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcTempTE.TextChanged
        Call VerifyMETEEntry(txtCalcTempTE, "TE")
    End Sub

    Private Sub chkCalcAutoCalcT2NumBPs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcAutoCalcT2NumBPs.CheckedChanged
        Call ResetRefresh()
    End Sub

    Private Sub lstManufacturing_ColumnClick(sender As System.Object, e As System.Windows.Forms.ColumnClickEventArgs) Handles lstManufacturing.ColumnClick

        Call ListViewColumnSorter(e.Column, CType(lstManufacturing, ListView), ManufacturingColumnClicked, ManufacturingColumnSortType)

    End Sub

    Private Sub txtCalcIPHThreshold_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcIPHThreshold.KeyPress
        ' Only allow numbers, decimal, negative or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedDecimalChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcIPHThreshold_LostFocus(sender As Object, e As System.EventArgs) Handles txtCalcIPHThreshold.LostFocus
        txtCalcIPHThreshold.Text = FormatNumber(txtCalcIPHThreshold.Text, 2)
    End Sub

    Private Sub txtCalcProfitThreshold_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcProfitThreshold.KeyPress

        If chkCalcProfitThreshold.Text.Contains("%") Then
            ' Only allow numbers, decimal, negative or backspace
            If e.KeyChar <> ControlChars.Back Then
                If allowedPercentChars.IndexOf(e.KeyChar) = -1 Then
                    ' Invalid Character
                    e.Handled = True
                Else
                    ProfitPercentText = txtCalcProfitThreshold.Text
                    Call ResetRefresh()
                End If
            End If
        Else
            ' Only allow numbers, decimal, negative or backspace
            If e.KeyChar <> ControlChars.Back Then
                If allowedDecimalChars.IndexOf(e.KeyChar) = -1 Then
                    ' Invalid Character
                    e.Handled = True
                Else
                    ProfitText = txtCalcProfitThreshold.Text
                    Call ResetRefresh()
                End If
            End If
        End If
    End Sub

    Private Sub txtCalcProfitThreshold_LostFocus(sender As Object, e As System.EventArgs) Handles txtCalcProfitThreshold.LostFocus

        If chkCalcProfitThreshold.Text.Contains("%") Then
            txtCalcProfitThreshold.Text = FormatPercent(CDbl(txtCalcProfitThreshold.Text.Replace("%", "")) / 100, 1)
        Else
            txtCalcProfitThreshold.Text = FormatNumber(txtCalcProfitThreshold.Text, 2)
        End If
    End Sub

    Private Sub txtCalcVolumeThreshold_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtCalcVolumeThreshold.KeyPress
        ' Only allow postive numbers
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            Else
                Call ResetRefresh()
            End If
        End If
    End Sub

    Private Sub txtCalcVolumeThreshold_LostFocus(sender As Object, e As System.EventArgs) Handles txtCalcVolumeThreshold.LostFocus
        txtCalcVolumeThreshold.Text = FormatNumber(txtCalcVolumeThreshold.Text, 0)
    End Sub

    Private Sub cmbCalcPriceTrend_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbCalcPriceTrend.SelectedIndexChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcIPHThreshold_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcIPHThreshold.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcProfitThreshold_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcProfitThreshold.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub txtCalcVolumeThreshold_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtCalcVolumeThreshold.TextChanged
        Call ResetRefresh()
    End Sub

    Private Sub chkCalcMinBuildTimeFilter_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcMinBuildTimeFilter.CheckedChanged
        Call ResetRefresh()
        If chkCalcMinBuildTimeFilter.Checked Then
            tpMinBuildTimeFilter.Enabled = True
        Else
            tpMinBuildTimeFilter.Enabled = False
        End If
    End Sub

    Private Sub chkCalcMaxBuildTimeFilter_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcMaxBuildTimeFilter.CheckedChanged
        Call ResetRefresh()
        If chkCalcMaxBuildTimeFilter.Checked Then
            tpMaxBuildTimeFilter.Enabled = True
        Else
            tpMaxBuildTimeFilter.Enabled = False
        End If
    End Sub

    Private Sub chkCalcIPHThreshold_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcIPHThreshold.CheckedChanged
        Call ResetRefresh()
        If chkCalcIPHThreshold.Checked Then
            txtCalcIPHThreshold.Enabled = True
        Else
            txtCalcIPHThreshold.Enabled = False
        End If
    End Sub

    Private Sub chkCalcProfitThreshold_Click(sender As Object, e As EventArgs) Handles chkCalcProfitThreshold.Click

        ' Save the value
        If chkCalcProfitThreshold.Text.Contains("%") Then
            ProfitPercentText = txtCalcProfitThreshold.Text
        Else
            ProfitText = txtCalcProfitThreshold.Text
        End If

        ' Change the name based on the check state
        If chkCalcProfitThreshold.CheckState <> CheckState.Indeterminate Then
            chkCalcProfitThreshold.Text = "Profit Threshold:"
            txtCalcProfitThreshold.Text = FormatNumber(ProfitText, 2)
        ElseIf chkCalcProfitThreshold.CheckState = CheckState.Indeterminate Then
            chkCalcProfitThreshold.Text = "Profit % Threshold:"
            txtCalcProfitThreshold.Text = FormatPercent(CDbl(ProfitPercentText.Replace("%", "")) / 100)
        End If

        If Not FirstLoad Then
            ' For this one, it's optimal so disable all the others if checked
            If chkCalcProfitThreshold.CheckState <> CheckState.Unchecked Then
                txtCalcProfitThreshold.Enabled = True
            Else
                txtCalcProfitThreshold.Enabled = False
            End If
            Call ResetRefresh()
        End If
    End Sub

    Private Sub chkCalcVolumeThreshold_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkCalcVolumeThreshold.CheckedChanged
        Call ResetRefresh()
        If chkCalcVolumeThreshold.Checked Then
            txtCalcVolumeThreshold.Enabled = True
        Else
            txtCalcVolumeThreshold.Enabled = False
        End If
    End Sub

    Private Sub tpMinBuildTimeFilter_TimeChange(sender As Object, e As System.EventArgs) Handles tpMinBuildTimeFilter.TimeChange
        Call ResetRefresh()
    End Sub

    Private Sub tpMaxBuildTimeFilter_TimeChange(sender As Object, e As System.EventArgs) Handles tpMaxBuildTimeFilter.TimeChange
        Call ResetRefresh()
    End Sub

#End Region

#Region "Column Select Functions"

    ' Clears the list and rebuilds it with columns they selected
    Private Sub RefreshManufacturingTabColumns()

        Call LoadManufacturingTabColumnPositions()
        Call lstManufacturing.Clear()
        AddingColumns = True

        ' Add the first hidden column
        lstManufacturing.Columns.Add("ListID")
        lstManufacturing.Columns(0).Width = 0

        ' Now load all the columns in order of the settings
        For i = 1 To ColumnPositions.Count - 1
            If ColumnPositions(i) <> "" Then
                lstManufacturing.Columns.Add(ColumnPositions(i), GetColumnWidth(ColumnPositions(i)), GetColumnAlignment(ColumnPositions(i)))
            End If
        Next

        ' Hack to get around the scroll bar not showing
        lstManufacturing.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.None)

        AddingColumns = False

    End Sub

    ' Takes the column settings and saves the order to an array
    Private Sub LoadManufacturingTabColumnPositions()

        For i = 0 To ColumnPositions.Count - 1
            ColumnPositions(i) = ""
        Next

        With UserManufacturingTabColumnSettings
            ColumnPositions(.ItemCategory) = ProgramSettings.ItemCategoryColumnName
            ColumnPositions(.ItemGroup) = ProgramSettings.ItemGroupColumnName
            ColumnPositions(.ItemName) = ProgramSettings.ItemNameColumnName
            ColumnPositions(.Owned) = ProgramSettings.OwnedColumnName
            ColumnPositions(.Tech) = ProgramSettings.TechColumnName
            ColumnPositions(.BPME) = ProgramSettings.BPMEColumnName
            ColumnPositions(.BPTE) = ProgramSettings.BPTEColumnName
            ColumnPositions(.Inputs) = ProgramSettings.InputsColumnName
            ColumnPositions(.Compared) = ProgramSettings.ComparedColumnName
            ColumnPositions(.TotalRuns) = ProgramSettings.TotalRunsColumnName
            ColumnPositions(.SingleInventedBPCRuns) = ProgramSettings.SingleInventedBPCRunsColumnName
            ColumnPositions(.ProductionLines) = ProgramSettings.ProductionLinesColumnName
            ColumnPositions(.LaboratoryLines) = ProgramSettings.LaboratoryLinesColumnName
            ColumnPositions(.TotalInventionCost) = ProgramSettings.TotalInventionCostColumnName
            ColumnPositions(.TotalCopyCost) = ProgramSettings.TotalCopyCostColumnName
            ColumnPositions(.Taxes) = ProgramSettings.TaxesColumnName
            ColumnPositions(.BrokerFees) = ProgramSettings.BrokerFeesColumnName
            ColumnPositions(.BPProductionTime) = ProgramSettings.BPProductionTimeColumnName
            ColumnPositions(.TotalProductionTime) = ProgramSettings.TotalProductionTimeColumnName
            ColumnPositions(.CopyTime) = ProgramSettings.CopyTimeColumnName
            ColumnPositions(.InventionTime) = ProgramSettings.InventionTimeColumnName
            ColumnPositions(.ItemMarketPrice) = ProgramSettings.ItemMarketPriceColumnName
            ColumnPositions(.Profit) = ProgramSettings.ProfitColumnName
            ColumnPositions(.ProfitPercentage) = ProgramSettings.ProfitPercentageColumnName
            ColumnPositions(.IskperHour) = ProgramSettings.IskperHourColumnName
            ColumnPositions(.SVR) = ProgramSettings.SVRColumnName
            ColumnPositions(.SVRxIPH) = ProgramSettings.SVRxIPHColumnName
            ColumnPositions(.PriceTrend) = ProgramSettings.PriceTrendColumnName
            ColumnPositions(.TotalItemsSold) = ProgramSettings.TotalItemsSoldColumnName
            ColumnPositions(.TotalOrdersFilled) = ProgramSettings.TotalOrdersFilledColumnName
            ColumnPositions(.AvgItemsperOrder) = ProgramSettings.AvgItemsperOrderColumnName
            ColumnPositions(.CurrentSellOrders) = ProgramSettings.CurrentSellOrdersColumnName
            ColumnPositions(.CurrentBuyOrders) = ProgramSettings.CurrentBuyOrdersColumnName
            ColumnPositions(.ItemsinStock) = ProgramSettings.ItemsinStockColumnName
            ColumnPositions(.ItemsinProduction) = ProgramSettings.ItemsinProductionColumnName
            ColumnPositions(.TotalCost) = ProgramSettings.TotalCostColumnName
            ColumnPositions(.BaseJobCost) = ProgramSettings.BaseJobCostColumnName
            ColumnPositions(.NumBPs) = ProgramSettings.NumBPsColumnName
            ColumnPositions(.InventionChance) = ProgramSettings.InventionChanceColumnName
            ColumnPositions(.BPType) = ProgramSettings.BPTypeColumnName
            ColumnPositions(.Race) = ProgramSettings.RaceColumnName
            ColumnPositions(.VolumeperItem) = ProgramSettings.VolumeperItemColumnName
            ColumnPositions(.TotalVolume) = ProgramSettings.TotalVolumeColumnName
            ColumnPositions(.PortionSize) = ProgramSettings.PortionSizeColumnName
            ColumnPositions(.ManufacturingJobFee) = ProgramSettings.ManufacturingJobFeeColumnName
            ColumnPositions(.ManufacturingFacilityName) = ProgramSettings.ManufacturingFacilityNameColumnName
            ColumnPositions(.ManufacturingFacilitySystem) = ProgramSettings.ManufacturingFacilitySystemColumnName
            ColumnPositions(.ManufacturingFacilityRegion) = ProgramSettings.ManufacturingFacilityRegionColumnName
            ColumnPositions(.ManufacturingFacilitySystemIndex) = ProgramSettings.ManufacturingFacilitySystemIndexColumnName
            ColumnPositions(.ManufacturingFacilityTax) = ProgramSettings.ManufacturingFacilityTaxColumnName
            ColumnPositions(.ManufacturingFacilityMEBonus) = ProgramSettings.ManufacturingFacilityMEBonusColumnName
            ColumnPositions(.ManufacturingFacilityTEBonus) = ProgramSettings.ManufacturingFacilityTEBonusColumnName
            ColumnPositions(.ManufacturingFacilityUsage) = ProgramSettings.ManufacturingFacilityUsageColumnName
            ColumnPositions(.ManufacturingFacilityFWSystemLevel) = ProgramSettings.ManufacturingFacilityFWSystemLevelColumnName
            ColumnPositions(.ComponentFacilityName) = ProgramSettings.ComponentFacilityNameColumnName
            ColumnPositions(.ComponentFacilitySystem) = ProgramSettings.ComponentFacilitySystemColumnName
            ColumnPositions(.ComponentFacilityRegion) = ProgramSettings.ComponentFacilityRegionColumnName
            ColumnPositions(.ComponentFacilitySystemIndex) = ProgramSettings.ComponentFacilitySystemIndexColumnName
            ColumnPositions(.ComponentFacilityTax) = ProgramSettings.ComponentFacilityTaxColumnName
            ColumnPositions(.ComponentFacilityMEBonus) = ProgramSettings.ComponentFacilityMEBonusColumnName
            ColumnPositions(.ComponentFacilityTEBonus) = ProgramSettings.ComponentFacilityTEBonusColumnName
            ColumnPositions(.ComponentFacilityUsage) = ProgramSettings.ComponentFacilityUsageColumnName
            ColumnPositions(.ComponentFacilityFWSystemLevel) = ProgramSettings.ComponentFacilityFWSystemLevelColumnName
            ColumnPositions(.CapComponentFacilityName) = ProgramSettings.CapComponentFacilityNameColumnName
            ColumnPositions(.CapComponentFacilitySystem) = ProgramSettings.CapComponentFacilitySystemColumnName
            ColumnPositions(.CapComponentFacilityRegion) = ProgramSettings.CapComponentFacilityRegionColumnName
            ColumnPositions(.CapComponentFacilitySystemIndex) = ProgramSettings.CapComponentFacilitySystemIndexColumnName
            ColumnPositions(.CapComponentFacilityTax) = ProgramSettings.CapComponentFacilityTaxColumnName
            ColumnPositions(.CapComponentFacilityMEBonus) = ProgramSettings.CapComponentFacilityMEBonusColumnName
            ColumnPositions(.CapComponentFacilityTEBonus) = ProgramSettings.CapComponentFacilityTEBonusColumnName
            ColumnPositions(.CapComponentFacilityUsage) = ProgramSettings.CapComponentFacilityUsageColumnName
            ColumnPositions(.CapComponentFacilityFWSystemLevel) = ProgramSettings.CapComponentFacilityFWSystemLevelColumnName
            ColumnPositions(.CopyingFacilityName) = ProgramSettings.CopyingFacilityNameColumnName
            ColumnPositions(.CopyingFacilitySystem) = ProgramSettings.CopyingFacilitySystemColumnName
            ColumnPositions(.CopyingFacilityRegion) = ProgramSettings.CopyingFacilityRegionColumnName
            ColumnPositions(.CopyingFacilitySystemIndex) = ProgramSettings.CopyingFacilitySystemIndexColumnName
            ColumnPositions(.CopyingFacilityTax) = ProgramSettings.CopyingFacilityTaxColumnName
            ColumnPositions(.CopyingFacilityMEBonus) = ProgramSettings.CopyingFacilityMEBonusColumnName
            ColumnPositions(.CopyingFacilityTEBonus) = ProgramSettings.CopyingFacilityTEBonusColumnName
            ColumnPositions(.CopyingFacilityUsage) = ProgramSettings.CopyingFacilityUsageColumnName
            ColumnPositions(.CopyingFacilityFWSystemLevel) = ProgramSettings.CopyingFacilityFWSystemLevelColumnName
            ColumnPositions(.InventionFacilityName) = ProgramSettings.InventionFacilityNameColumnName
            ColumnPositions(.InventionFacilitySystem) = ProgramSettings.InventionFacilitySystemColumnName
            ColumnPositions(.InventionFacilityRegion) = ProgramSettings.InventionFacilityRegionColumnName
            ColumnPositions(.InventionFacilitySystemIndex) = ProgramSettings.InventionFacilitySystemIndexColumnName
            ColumnPositions(.InventionFacilityTax) = ProgramSettings.InventionFacilityTaxColumnName
            ColumnPositions(.InventionFacilityMEBonus) = ProgramSettings.InventionFacilityMEBonusColumnName
            ColumnPositions(.InventionFacilityTEBonus) = ProgramSettings.InventionFacilityTEBonusColumnName
            ColumnPositions(.InventionFacilityUsage) = ProgramSettings.InventionFacilityUsageColumnName
            ColumnPositions(.InventionFacilityFWSystemLevel) = ProgramSettings.InventionFacilityFWSystemLevelColumnName
        End With

        ' First column is always the ListID
        ColumnPositions(0) = "ListID"

    End Sub

    ' Returns the column Width for the sent column name
    Private Function GetColumnWidth(ColumnName As String) As Integer

        With UserManufacturingTabColumnSettings
            Select Case ColumnName
                Case ProgramSettings.ItemCategoryColumnName
                    Return .ItemCategoryWidth
                Case ProgramSettings.ItemGroupColumnName
                    Return .ItemGroupWidth
                Case ProgramSettings.ItemNameColumnName
                    Return .ItemNameWidth
                Case ProgramSettings.OwnedColumnName
                    Return .OwnedWidth
                Case ProgramSettings.TechColumnName
                    Return .TechWidth
                Case ProgramSettings.BPMEColumnName
                    Return .BPMEWidth
                Case ProgramSettings.BPTEColumnName
                    Return .BPTEWidth
                Case ProgramSettings.InputsColumnName
                    Return .InputsWidth
                Case ProgramSettings.ComparedColumnName
                    Return .ComparedWidth
                Case ProgramSettings.TotalRunsColumnName
                    Return .TotalRunsWidth
                Case ProgramSettings.SingleInventedBPCRunsColumnName
                    Return .SingleInventedBPCRunsWidth
                Case ProgramSettings.ProductionLinesColumnName
                    Return .ProductionLinesWidth
                Case ProgramSettings.LaboratoryLinesColumnName
                    Return .LaboratoryLinesWidth
                Case ProgramSettings.TotalInventionCostColumnName
                    Return .TotalInventionCostWidth
                Case ProgramSettings.TotalCopyCostColumnName
                    Return .TotalCopyCostWidth
                Case ProgramSettings.TaxesColumnName
                    Return .TaxesWidth
                Case ProgramSettings.BrokerFeesColumnName
                    Return .BrokerFeesWidth
                Case ProgramSettings.BPProductionTimeColumnName
                    Return .BPProductionTimeWidth
                Case ProgramSettings.TotalProductionTimeColumnName
                    Return .TotalProductionTimeWidth
                Case ProgramSettings.CopyTimeColumnName
                    Return .CopyTimeWidth
                Case ProgramSettings.InventionTimeColumnName
                    Return .InventionTimeWidth
                Case ProgramSettings.ItemMarketPriceColumnName
                    Return .ItemMarketPriceWidth
                Case ProgramSettings.ProfitColumnName
                    Return .ProfitWidth
                Case ProgramSettings.ProfitPercentageColumnName
                    Return .ProfitPercentageWidth
                Case ProgramSettings.IskperHourColumnName
                    Return .IskperHourWidth
                Case ProgramSettings.SVRColumnName
                    Return .SVRWidth
                Case ProgramSettings.SVRxIPHColumnName
                    Return .SVRxIPHWidth
                Case ProgramSettings.PriceTrendColumnName
                    Return .PriceTrendWidth
                Case ProgramSettings.TotalItemsSoldColumnName
                    Return .TotalItemsSoldWidth
                Case ProgramSettings.TotalOrdersFilledColumnName
                    Return .TotalOrdersFilledWidth
                Case ProgramSettings.ItemsinProductionColumnName
                    Return .ItemsinProductionWidth
                Case ProgramSettings.ItemsinStockColumnName
                    Return .ItemsinStockWidth
                Case ProgramSettings.AvgItemsperOrderColumnName
                    Return .AvgItemsperOrderWidth
                Case ProgramSettings.CurrentSellOrdersColumnName
                    Return .CurrentSellOrdersWidth
                Case ProgramSettings.CurrentBuyOrdersColumnName
                    Return .CurrentBuyOrdersWidth
                Case ProgramSettings.TotalCostColumnName
                    Return .TotalCostWidth
                Case ProgramSettings.BaseJobCostColumnName
                    Return .BaseJobCostWidth
                Case ProgramSettings.NumBPsColumnName
                    Return .NumBPsWidth
                Case ProgramSettings.InventionChanceColumnName
                    Return .InventionChanceWidth
                Case ProgramSettings.BPTypeColumnName
                    Return .BPTypeWidth
                Case ProgramSettings.RaceColumnName
                    Return .RaceWidth
                Case ProgramSettings.VolumeperItemColumnName
                    Return .VolumeperItemWidth
                Case ProgramSettings.TotalVolumeColumnName
                    Return .TotalVolumeWidth
                Case ProgramSettings.PortionSizeColumnName
                    Return .PortionSizeWidth
                Case ProgramSettings.ManufacturingJobFeeColumnName
                    Return .ManufacturingJobFeeWidth
                Case ProgramSettings.ManufacturingFacilityNameColumnName
                    Return .ManufacturingFacilityNameWidth
                Case ProgramSettings.ManufacturingFacilitySystemColumnName
                    Return .ManufacturingFacilitySystemWidth
                Case ProgramSettings.ManufacturingFacilityRegionColumnName
                    Return .ManufacturingFacilityRegionWidth
                Case ProgramSettings.ManufacturingFacilitySystemIndexColumnName
                    Return .ManufacturingFacilitySystemIndexWidth
                Case ProgramSettings.ManufacturingFacilityTaxColumnName
                    Return .ManufacturingFacilityTaxWidth
                Case ProgramSettings.ManufacturingFacilityMEBonusColumnName
                    Return .ManufacturingFacilityMEBonusWidth
                Case ProgramSettings.ManufacturingFacilityTEBonusColumnName
                    Return .ManufacturingFacilityTEBonusWidth
                Case ProgramSettings.ManufacturingFacilityUsageColumnName
                    Return .ManufacturingFacilityUsageWidth
                Case ProgramSettings.ManufacturingFacilityFWSystemLevelColumnName
                    Return .ManufacturingFacilityFWSystemLevelWidth
                Case ProgramSettings.ComponentFacilityNameColumnName
                    Return .ComponentFacilityNameWidth
                Case ProgramSettings.ComponentFacilitySystemColumnName
                    Return .ComponentFacilitySystemWidth
                Case ProgramSettings.ComponentFacilityRegionColumnName
                    Return .ComponentFacilityRegionWidth
                Case ProgramSettings.ComponentFacilitySystemIndexColumnName
                    Return .ComponentFacilitySystemIndexWidth
                Case ProgramSettings.ComponentFacilityTaxColumnName
                    Return .ComponentFacilityTaxWidth
                Case ProgramSettings.ComponentFacilityMEBonusColumnName
                    Return .ComponentFacilityMEBonusWidth
                Case ProgramSettings.ComponentFacilityTEBonusColumnName
                    Return .ComponentFacilityTEBonusWidth
                Case ProgramSettings.ComponentFacilityUsageColumnName
                    Return .ComponentFacilityUsageWidth
                Case ProgramSettings.ComponentFacilityFWSystemLevelColumnName
                    Return .ComponentFacilityFWSystemLevelWidth
                Case ProgramSettings.CapComponentFacilityNameColumnName
                    Return .CapComponentFacilityNameWidth
                Case ProgramSettings.CapComponentFacilitySystemColumnName
                    Return .CapComponentFacilitySystemWidth
                Case ProgramSettings.CapComponentFacilityRegionColumnName
                    Return .CapComponentFacilityRegionWidth
                Case ProgramSettings.CapComponentFacilitySystemIndexColumnName
                    Return .CapComponentFacilitySystemIndexWidth
                Case ProgramSettings.CapComponentFacilityTaxColumnName
                    Return .CapComponentFacilityTaxWidth
                Case ProgramSettings.CapComponentFacilityMEBonusColumnName
                    Return .CapComponentFacilityMEBonusWidth
                Case ProgramSettings.CapComponentFacilityTEBonusColumnName
                    Return .CapComponentFacilityTEBonusWidth
                Case ProgramSettings.CapComponentFacilityUsageColumnName
                    Return .CapComponentFacilityUsageWidth
                Case ProgramSettings.CapComponentFacilityFWSystemLevelColumnName
                    Return .CapComponentFacilityFWSystemLevelWidth
                Case ProgramSettings.CopyingFacilityNameColumnName
                    Return .CopyingFacilityNameWidth
                Case ProgramSettings.CopyingFacilitySystemColumnName
                    Return .CopyingFacilitySystemWidth
                Case ProgramSettings.CopyingFacilityRegionColumnName
                    Return .CopyingFacilityRegionWidth
                Case ProgramSettings.CopyingFacilitySystemIndexColumnName
                    Return .CopyingFacilitySystemIndexWidth
                Case ProgramSettings.CopyingFacilityTaxColumnName
                    Return .CopyingFacilityTaxWidth
                Case ProgramSettings.CopyingFacilityMEBonusColumnName
                    Return .CopyingFacilityMEBonusWidth
                Case ProgramSettings.CopyingFacilityTEBonusColumnName
                    Return .CopyingFacilityTEBonusWidth
                Case ProgramSettings.CopyingFacilityUsageColumnName
                    Return .CopyingFacilityUsageWidth
                Case ProgramSettings.CopyingFacilityFWSystemLevelColumnName
                    Return .CopyingFacilityFWSystemLevelWidth
                Case ProgramSettings.InventionFacilityNameColumnName
                    Return .InventionFacilityNameWidth
                Case ProgramSettings.InventionFacilitySystemColumnName
                    Return .InventionFacilitySystemWidth
                Case ProgramSettings.InventionFacilityRegionColumnName
                    Return .InventionFacilityRegionWidth
                Case ProgramSettings.InventionFacilitySystemIndexColumnName
                    Return .InventionFacilitySystemIndexWidth
                Case ProgramSettings.InventionFacilityTaxColumnName
                    Return .InventionFacilityTaxWidth
                Case ProgramSettings.InventionFacilityMEBonusColumnName
                    Return .InventionFacilityMEBonusWidth
                Case ProgramSettings.InventionFacilityTEBonusColumnName
                    Return .InventionFacilityTEBonusWidth
                Case ProgramSettings.InventionFacilityUsageColumnName
                    Return .InventionFacilityUsageWidth
                Case ProgramSettings.InventionFacilityFWSystemLevelColumnName
                    Return .InventionFacilityFWSystemLevelWidth
                Case Else
                    Return 0
            End Select
        End With

    End Function

    ' Updates the column order when changed
    Private Sub lstManufacturing_ColumnReordered(sender As Object, e As System.Windows.Forms.ColumnReorderedEventArgs) Handles lstManufacturing.ColumnReordered
        Dim TempArray(NumManufacturingTabColumns) As String
        Dim Minus1 As Boolean = False

        e.Cancel = True ' Cancel the event so we can manually update the grid columns

        For i = 0 To NumManufacturingTabColumns
            TempArray(i) = ""
        Next

        ' First index is the ListID
        TempArray(0) = "ListID"

        If e.OldDisplayIndex > e.NewDisplayIndex Then
            ' For all indices larger than the new index, need to move it to the next array
            For i = 1 To e.NewDisplayIndex - 1
                TempArray(i) = ColumnPositions(i)
            Next

            ' Insert the new column
            TempArray(e.NewDisplayIndex) = ColumnPositions(e.OldDisplayIndex)

            ' Move all the rest of the items up one
            For i = e.NewDisplayIndex + 1 To TempArray.Count - 1
                If i < e.OldDisplayIndex + 1 Then
                    TempArray(i) = ColumnPositions(i - 1)
                Else
                    TempArray(i) = ColumnPositions(i)
                End If
            Next
        Else
            ' For all indices larger than the new index, need to move it to the next array
            For i = 1 To e.OldDisplayIndex - 1
                TempArray(i) = ColumnPositions(i)
            Next

            ' Insert the new column
            TempArray(e.NewDisplayIndex) = ColumnPositions(e.OldDisplayIndex)

            ' Back fill the array between the column we moved and the new location
            For i = e.OldDisplayIndex To e.NewDisplayIndex - 1
                TempArray(i) = ColumnPositions(i + 1)
            Next

            ' Replace all the items left
            For i = e.NewDisplayIndex + 1 To TempArray.Count - 1
                TempArray(i) = ColumnPositions(i)
            Next

        End If

        ColumnPositions = TempArray

        ' Save the columns based on the current order
        With UserManufacturingTabColumnSettings
            For i = 1 To ColumnPositions.Count - 1
                Select Case ColumnPositions(i)
                    Case ProgramSettings.ItemCategoryColumnName
                        .ItemCategory = i
                    Case ProgramSettings.ItemGroupColumnName
                        .ItemGroup = i
                    Case ProgramSettings.ItemNameColumnName
                        .ItemName = i
                    Case ProgramSettings.OwnedColumnName
                        .Owned = i
                    Case ProgramSettings.TechColumnName
                        .Tech = i
                    Case ProgramSettings.BPMEColumnName
                        .BPME = i
                    Case ProgramSettings.BPTEColumnName
                        .BPTE = i
                    Case ProgramSettings.InputsColumnName
                        .Inputs = i
                    Case ProgramSettings.ComparedColumnName
                        .Compared = i
                    Case ProgramSettings.TotalRunsColumnName
                        .TotalRuns = i
                    Case ProgramSettings.SingleInventedBPCRunsColumnName
                        .SingleInventedBPCRuns = i
                    Case ProgramSettings.ProductionLinesColumnName
                        .ProductionLines = i
                    Case ProgramSettings.LaboratoryLinesColumnName
                        .LaboratoryLines = i
                    Case ProgramSettings.TotalInventionCostColumnName
                        .TotalInventionCost = i
                    Case ProgramSettings.TotalCopyCostColumnName
                        .TotalCopyCost = i
                    Case ProgramSettings.TaxesColumnName
                        .Taxes = i
                    Case ProgramSettings.BrokerFeesColumnName
                        .BrokerFees = i
                    Case ProgramSettings.BPProductionTimeColumnName
                        .BPProductionTime = i
                    Case ProgramSettings.TotalProductionTimeColumnName
                        .TotalProductionTime = i
                    Case ProgramSettings.CopyTimeColumnName
                        .CopyTime = i
                    Case ProgramSettings.InventionTimeColumnName
                        .InventionTime = i
                    Case ProgramSettings.ItemMarketPriceColumnName
                        .ItemMarketPrice = i
                    Case ProgramSettings.ProfitColumnName
                        .Profit = i
                    Case ProgramSettings.ProfitPercentageColumnName
                        .ProfitPercentage = i
                    Case ProgramSettings.IskperHourColumnName
                        .IskperHour = i
                    Case ProgramSettings.SVRColumnName
                        .SVR = i
                    Case ProgramSettings.SVRxIPHColumnName
                        .SVRxIPH = i
                    Case ProgramSettings.PriceTrendColumnName
                        .PriceTrend = i
                    Case ProgramSettings.TotalItemsSoldColumnName
                        .TotalItemsSold = i
                    Case ProgramSettings.TotalOrdersFilledColumnName
                        .TotalOrdersFilled = i
                    Case ProgramSettings.AvgItemsperOrderColumnName
                        .AvgItemsperOrder = i
                    Case ProgramSettings.CurrentSellOrdersColumnName
                        .CurrentSellOrders = i
                    Case ProgramSettings.CurrentBuyOrdersColumnName
                        .CurrentBuyOrders = i
                    Case ProgramSettings.TotalCostColumnName
                        .TotalCost = i
                    Case ProgramSettings.BaseJobCostColumnName
                        .BaseJobCost = i
                    Case ProgramSettings.NumBPsColumnName
                        .NumBPs = i
                    Case ProgramSettings.InventionChanceColumnName
                        .InventionChance = i
                    Case ProgramSettings.BPTypeColumnName
                        .BPType = i
                    Case ProgramSettings.RaceColumnName
                        .Race = i
                    Case ProgramSettings.VolumeperItemColumnName
                        .VolumeperItem = i
                    Case ProgramSettings.TotalVolumeColumnName
                        .TotalVolume = i
                    Case ProgramSettings.PortionSizeColumnName
                        .PortionSize = i
                    Case ProgramSettings.ManufacturingJobFeeColumnName
                        .ManufacturingJobFee = i
                    Case ProgramSettings.ManufacturingFacilityNameColumnName
                        .ManufacturingFacilityName = i
                    Case ProgramSettings.ManufacturingFacilitySystemColumnName
                        .ManufacturingFacilitySystem = i
                    Case ProgramSettings.ManufacturingFacilityRegionColumnName
                        .ManufacturingFacilityRegion = i
                    Case ProgramSettings.ManufacturingFacilitySystemIndexColumnName
                        .ManufacturingFacilitySystemIndex = i
                    Case ProgramSettings.ManufacturingFacilityTaxColumnName
                        .ManufacturingFacilityTax = i
                    Case ProgramSettings.ManufacturingFacilityMEBonusColumnName
                        .ManufacturingFacilityMEBonus = i
                    Case ProgramSettings.ManufacturingFacilityTEBonusColumnName
                        .ManufacturingFacilityTEBonus = i
                    Case ProgramSettings.ManufacturingFacilityUsageColumnName
                        .ManufacturingFacilityUsage = i
                    Case ProgramSettings.ManufacturingFacilityFWSystemLevelColumnName
                        .ManufacturingFacilityFWSystemLevel = i
                    Case ProgramSettings.ComponentFacilityNameColumnName
                        .ComponentFacilityName = i
                    Case ProgramSettings.ComponentFacilitySystemColumnName
                        .ComponentFacilitySystem = i
                    Case ProgramSettings.ComponentFacilityRegionColumnName
                        .ComponentFacilityRegion = i
                    Case ProgramSettings.ComponentFacilitySystemIndexColumnName
                        .ComponentFacilitySystemIndex = i
                    Case ProgramSettings.ComponentFacilityTaxColumnName
                        .ComponentFacilityTax = i
                    Case ProgramSettings.ComponentFacilityMEBonusColumnName
                        .ComponentFacilityMEBonus = i
                    Case ProgramSettings.ComponentFacilityTEBonusColumnName
                        .ComponentFacilityTEBonus = i
                    Case ProgramSettings.ComponentFacilityUsageColumnName
                        .ComponentFacilityUsage = i
                    Case ProgramSettings.ComponentFacilityFWSystemLevelColumnName
                        .ComponentFacilityFWSystemLevel = i
                    Case ProgramSettings.CapComponentFacilityNameColumnName
                        .CapComponentFacilityName = i
                    Case ProgramSettings.CapComponentFacilitySystemColumnName
                        .CapComponentFacilitySystem = i
                    Case ProgramSettings.CapComponentFacilityRegionColumnName
                        .CapComponentFacilityRegion = i
                    Case ProgramSettings.CapComponentFacilitySystemIndexColumnName
                        .CapComponentFacilitySystemIndex = i
                    Case ProgramSettings.CapComponentFacilityTaxColumnName
                        .CapComponentFacilityTax = i
                    Case ProgramSettings.CapComponentFacilityMEBonusColumnName
                        .CapComponentFacilityMEBonus = i
                    Case ProgramSettings.CapComponentFacilityTEBonusColumnName
                        .CapComponentFacilityTEBonus = i
                    Case ProgramSettings.CapComponentFacilityUsageColumnName
                        .CapComponentFacilityUsage = i
                    Case ProgramSettings.CapComponentFacilityFWSystemLevelColumnName
                        .CapComponentFacilityFWSystemLevel = i
                    Case ProgramSettings.CopyingFacilityNameColumnName
                        .CopyingFacilityName = i
                    Case ProgramSettings.CopyingFacilitySystemColumnName
                        .CopyingFacilitySystem = i
                    Case ProgramSettings.CopyingFacilityRegionColumnName
                        .CopyingFacilityRegion = i
                    Case ProgramSettings.CopyingFacilitySystemIndexColumnName
                        .CopyingFacilitySystemIndex = i
                    Case ProgramSettings.CopyingFacilityTaxColumnName
                        .CopyingFacilityTax = i
                    Case ProgramSettings.CopyingFacilityMEBonusColumnName
                        .CopyingFacilityMEBonus = i
                    Case ProgramSettings.CopyingFacilityTEBonusColumnName
                        .CopyingFacilityTEBonus = i
                    Case ProgramSettings.CopyingFacilityUsageColumnName
                        .CopyingFacilityUsage = i
                    Case ProgramSettings.CopyingFacilityFWSystemLevelColumnName
                        .CopyingFacilityFWSystemLevel = i
                    Case ProgramSettings.InventionFacilityNameColumnName
                        .InventionFacilityName = i
                    Case ProgramSettings.InventionFacilitySystemColumnName
                        .InventionFacilitySystem = i
                    Case ProgramSettings.InventionFacilityRegionColumnName
                        .InventionFacilityRegion = i
                    Case ProgramSettings.InventionFacilitySystemIndexColumnName
                        .InventionFacilitySystemIndex = i
                    Case ProgramSettings.InventionFacilityTaxColumnName
                        .InventionFacilityTax = i
                    Case ProgramSettings.InventionFacilityMEBonusColumnName
                        .InventionFacilityMEBonus = i
                    Case ProgramSettings.InventionFacilityTEBonusColumnName
                        .InventionFacilityTEBonus = i
                    Case ProgramSettings.InventionFacilityUsageColumnName
                        .InventionFacilityUsage = i
                    Case ProgramSettings.InventionFacilityFWSystemLevelColumnName
                        .InventionFacilityFWSystemLevel = i
                End Select
            Next
        End With

        ' Now Refresh the grid
        If lstManufacturing.Items.Count <> 0 Then
            RefreshCalcData = True
            Call DisplayManufacturingResults(False)
        Else
            Call RefreshManufacturingTabColumns()
        End If

    End Sub

    ' Updates the column sizes when changed
    Private Sub lstManufacturing_ColumnWidthChanged(sender As Object, e As System.Windows.Forms.ColumnWidthChangedEventArgs) Handles lstManufacturing.ColumnWidthChanged
        Dim NewWidth As Integer = lstManufacturing.Columns(e.ColumnIndex).Width

        If Not AddingColumns Then
            With UserManufacturingTabColumnSettings
                Select Case ColumnPositions(e.ColumnIndex)
                    Case ProgramSettings.ItemCategoryColumnName
                        .ItemCategoryWidth = NewWidth
                    Case ProgramSettings.ItemGroupColumnName
                        .ItemGroupWidth = NewWidth
                    Case ProgramSettings.ItemNameColumnName
                        .ItemNameWidth = NewWidth
                    Case ProgramSettings.OwnedColumnName
                        .OwnedWidth = NewWidth
                    Case ProgramSettings.TechColumnName
                        .TechWidth = NewWidth
                    Case ProgramSettings.BPMEColumnName
                        .BPMEWidth = NewWidth
                    Case ProgramSettings.BPTEColumnName
                        .BPTEWidth = NewWidth
                    Case ProgramSettings.InputsColumnName
                        .InputsWidth = NewWidth
                    Case ProgramSettings.ComparedColumnName
                        .ComparedWidth = NewWidth
                    Case ProgramSettings.TotalRunsColumnName
                        .TotalRunsWidth = NewWidth
                    Case ProgramSettings.SingleInventedBPCRunsColumnName
                        .SingleInventedBPCRunsWidth = NewWidth
                    Case ProgramSettings.ProductionLinesColumnName
                        .ProductionLinesWidth = NewWidth
                    Case ProgramSettings.LaboratoryLinesColumnName
                        .LaboratoryLinesWidth = NewWidth
                    Case ProgramSettings.TotalInventionCostColumnName
                        .TotalInventionCostWidth = NewWidth
                    Case ProgramSettings.TotalCopyCostColumnName
                        .TotalCopyCostWidth = NewWidth
                    Case ProgramSettings.TaxesColumnName
                        .TaxesWidth = NewWidth
                    Case ProgramSettings.BrokerFeesColumnName
                        .BrokerFeesWidth = NewWidth
                    Case ProgramSettings.BPProductionTimeColumnName
                        .BPProductionTimeWidth = NewWidth
                    Case ProgramSettings.TotalProductionTimeColumnName
                        .TotalProductionTimeWidth = NewWidth
                    Case ProgramSettings.CopyTimeColumnName
                        .CopyTimeWidth = NewWidth
                    Case ProgramSettings.InventionTimeColumnName
                        .InventionTimeWidth = NewWidth
                    Case ProgramSettings.ItemMarketPriceColumnName
                        .ItemMarketPriceWidth = NewWidth
                    Case ProgramSettings.ProfitColumnName
                        .ProfitWidth = NewWidth
                    Case ProgramSettings.ProfitPercentageColumnName
                        .ProfitPercentageWidth = NewWidth
                    Case ProgramSettings.IskperHourColumnName
                        .IskperHourWidth = NewWidth
                    Case ProgramSettings.SVRColumnName
                        .SVRWidth = NewWidth
                    Case ProgramSettings.SVRxIPHColumnName
                        .SVRxIPHWidth = NewWidth
                    Case ProgramSettings.PriceTrendColumnName
                        .PriceTrendWidth = NewWidth
                    Case ProgramSettings.TotalItemsSoldColumnName
                        .TotalItemsSoldWidth = NewWidth
                    Case ProgramSettings.TotalOrdersFilledColumnName
                        .TotalOrdersFilledWidth = NewWidth
                    Case ProgramSettings.AvgItemsperOrderColumnName
                        .AvgItemsperOrderWidth = NewWidth
                    Case ProgramSettings.CurrentSellOrdersColumnName
                        .CurrentSellOrdersWidth = NewWidth
                    Case ProgramSettings.CurrentBuyOrdersColumnName
                        .CurrentBuyOrdersWidth = NewWidth
                    Case ProgramSettings.TotalCostColumnName
                        .TotalCostWidth = NewWidth
                    Case ProgramSettings.BaseJobCostColumnName
                        .BaseJobCostWidth = NewWidth
                    Case ProgramSettings.NumBPsColumnName
                        .NumBPsWidth = NewWidth
                    Case ProgramSettings.InventionChanceColumnName
                        .InventionChanceWidth = NewWidth
                    Case ProgramSettings.BPTypeColumnName
                        .BPTypeWidth = NewWidth
                    Case ProgramSettings.RaceColumnName
                        .RaceWidth = NewWidth
                    Case ProgramSettings.VolumeperItemColumnName
                        .VolumeperItemWidth = NewWidth
                    Case ProgramSettings.TotalVolumeColumnName
                        .TotalVolumeWidth = NewWidth
                    Case ProgramSettings.PortionSizeColumnName
                        .PortionSizeWidth = NewWidth
                    Case ProgramSettings.ManufacturingJobFeeColumnName
                        .ManufacturingJobFeeWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityNameColumnName
                        .ManufacturingFacilityNameWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilitySystemColumnName
                        .ManufacturingFacilitySystemWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityRegionColumnName
                        .ManufacturingFacilityRegionWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilitySystemIndexColumnName
                        .ManufacturingFacilitySystemIndexWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityTaxColumnName
                        .ManufacturingFacilityTaxWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityMEBonusColumnName
                        .ManufacturingFacilityMEBonusWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityTEBonusColumnName
                        .ManufacturingFacilityTEBonusWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityUsageColumnName
                        .ManufacturingFacilityUsageWidth = NewWidth
                    Case ProgramSettings.ManufacturingFacilityFWSystemLevelColumnName
                        .ManufacturingFacilityFWSystemLevelWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityNameColumnName
                        .ComponentFacilityNameWidth = NewWidth
                    Case ProgramSettings.ComponentFacilitySystemColumnName
                        .ComponentFacilitySystemWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityRegionColumnName
                        .ComponentFacilityRegionWidth = NewWidth
                    Case ProgramSettings.ComponentFacilitySystemIndexColumnName
                        .ComponentFacilitySystemIndexWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityTaxColumnName
                        .ComponentFacilityTaxWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityMEBonusColumnName
                        .ComponentFacilityMEBonusWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityTEBonusColumnName
                        .ComponentFacilityTEBonusWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityUsageColumnName
                        .ComponentFacilityUsageWidth = NewWidth
                    Case ProgramSettings.ComponentFacilityFWSystemLevelColumnName
                        .ComponentFacilityFWSystemLevelWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityNameColumnName
                        .CapComponentFacilityNameWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilitySystemColumnName
                        .CapComponentFacilitySystemWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityRegionColumnName
                        .CapComponentFacilityRegionWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilitySystemIndexColumnName
                        .CapComponentFacilitySystemIndexWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityTaxColumnName
                        .CapComponentFacilityTaxWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityMEBonusColumnName
                        .CapComponentFacilityMEBonusWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityTEBonusColumnName
                        .CapComponentFacilityTEBonusWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityUsageColumnName
                        .CapComponentFacilityUsageWidth = NewWidth
                    Case ProgramSettings.CapComponentFacilityFWSystemLevelColumnName
                        .CapComponentFacilityFWSystemLevelWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityNameColumnName
                        .CopyingFacilityNameWidth = NewWidth
                    Case ProgramSettings.CopyingFacilitySystemColumnName
                        .CopyingFacilitySystemWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityRegionColumnName
                        .CopyingFacilityRegionWidth = NewWidth
                    Case ProgramSettings.CopyingFacilitySystemIndexColumnName
                        .CopyingFacilitySystemIndexWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityTaxColumnName
                        .CopyingFacilityTaxWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityMEBonusColumnName
                        .CopyingFacilityMEBonusWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityTEBonusColumnName
                        .CopyingFacilityTEBonusWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityUsageColumnName
                        .CopyingFacilityUsageWidth = NewWidth
                    Case ProgramSettings.CopyingFacilityFWSystemLevelColumnName
                        .CopyingFacilityFWSystemLevelWidth = NewWidth
                    Case ProgramSettings.InventionFacilityNameColumnName
                        .InventionFacilityNameWidth = NewWidth
                    Case ProgramSettings.InventionFacilitySystemColumnName
                        .InventionFacilitySystemWidth = NewWidth
                    Case ProgramSettings.InventionFacilityRegionColumnName
                        .InventionFacilityRegionWidth = NewWidth
                    Case ProgramSettings.InventionFacilitySystemIndexColumnName
                        .InventionFacilitySystemIndexWidth = NewWidth
                    Case ProgramSettings.InventionFacilityTaxColumnName
                        .InventionFacilityTaxWidth = NewWidth
                    Case ProgramSettings.InventionFacilityMEBonusColumnName
                        .InventionFacilityMEBonusWidth = NewWidth
                    Case ProgramSettings.InventionFacilityTEBonusColumnName
                        .InventionFacilityTEBonusWidth = NewWidth
                    Case ProgramSettings.InventionFacilityUsageColumnName
                        .InventionFacilityUsageWidth = NewWidth
                    Case ProgramSettings.InventionFacilityFWSystemLevelColumnName
                        .InventionFacilityFWSystemLevelWidth = NewWidth
                End Select
            End With
        End If

    End Sub

    ' Determines if we display the sent column
    Private Function ShowColumn(ColumnName As String) As Boolean
        If Array.IndexOf(ColumnPositions, ColumnName) <> -1 Then
            Return True
        Else
            Return False
        End If
    End Function

    ' Returns the allignment for the column name sent
    Private Function GetColumnAlignment(ColumnName As String) As HorizontalAlignment

        Select Case ColumnName
            Case ProgramSettings.ItemCategoryColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ItemGroupColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ItemNameColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.OwnedColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.TechColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.BPMEColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.BPTEColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InputsColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ComparedColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.TotalRunsColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.SingleInventedBPCRunsColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ProductionLinesColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.LaboratoryLinesColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalInventionCostColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalCopyCostColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TaxesColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.BrokerFeesColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.BPProductionTimeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalProductionTimeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CopyTimeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionTimeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ItemMarketPriceColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ProfitColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ProfitPercentageColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.IskperHourColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.SVRColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.SVRxIPHColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.PriceTrendColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalItemsSoldColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalOrdersFilledColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.AvgItemsperOrderColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CurrentSellOrdersColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CurrentBuyOrdersColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ItemsinProductionColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ItemsinStockColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalCostColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.BaseJobCostColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.NumBPsColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionChanceColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.BPTypeColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.RaceColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.VolumeperItemColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.TotalVolumeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.PortionSizeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ManufacturingJobFeeColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ManufacturingFacilityNameColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ManufacturingFacilitySystemColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ManufacturingFacilityRegionColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ManufacturingFacilitySystemIndexColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ManufacturingFacilityTaxColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ManufacturingFacilityMEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ManufacturingFacilityTEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ManufacturingFacilityUsageColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ManufacturingFacilityFWSystemLevelColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ComponentFacilityNameColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ComponentFacilitySystemColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ComponentFacilityRegionColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ComponentFacilitySystemIndexColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.ComponentFacilityTaxColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ComponentFacilityMEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ComponentFacilityTEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ComponentFacilityUsageColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.ComponentFacilityFWSystemLevelColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CapComponentFacilityNameColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CapComponentFacilitySystemColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CapComponentFacilityRegionColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CapComponentFacilitySystemIndexColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CapComponentFacilityTaxColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CapComponentFacilityMEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CapComponentFacilityTEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CapComponentFacilityUsageColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CapComponentFacilityFWSystemLevelColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CopyingFacilityNameColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CopyingFacilitySystemColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CopyingFacilityRegionColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CopyingFacilitySystemIndexColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.CopyingFacilityTaxColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CopyingFacilityMEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CopyingFacilityTEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CopyingFacilityUsageColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.CopyingFacilityFWSystemLevelColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionFacilityNameColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.InventionFacilitySystemColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.InventionFacilityRegionColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.InventionFacilitySystemIndexColumnName
                Return HorizontalAlignment.Left
            Case ProgramSettings.InventionFacilityTaxColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionFacilityMEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionFacilityTEBonusColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionFacilityUsageColumnName
                Return HorizontalAlignment.Right
            Case ProgramSettings.InventionFacilityFWSystemLevelColumnName
                Return HorizontalAlignment.Right
            Case Else
                Return 0
        End Select

    End Function

#End Region

    Private Sub InitManufacturingTab()

        lstManufacturing.Items.Clear()

        With UserManufacturingTabSettings
            ' Blueprints
            chkCalcAmmo.Checked = .CheckBPTypeAmmoCharges
            chkCalcBoosters.Checked = .CheckBPTypeBoosters
            chkCalcComponents.Checked = .CheckBPTypeComponents
            chkCalcDrones.Checked = .CheckBPTypeDrones
            chkCalcModules.Checked = .CheckBPTypeModules
            chkCalcRigs.Checked = .CheckBPTypeRigs
            chkCalcShips.Checked = .CheckBPTypeShips
            chkCalcSubsystems.Checked = .CheckBPTypeSubsystems
            chkCalcStructures.Checked = .CheckBPTypeStructures
            chkCalcMisc.Checked = .CheckBPTypeMisc
            chkCalcDeployables.Checked = .CheckBPTypeDeployables
            chkCalcCelestials.Checked = .CheckBPTypeCelestials
            chkCalcReactions.Checked = .CheckBPTypeReactions
            chkCalcStructureModules.Checked = .CheckBPTypeStructureModules
            chkCalcStructureRigs.Checked = .CheckBPTypeStationParts

            ' Tech
            chkCalcT1.Checked = .CheckTech1
            chkCalcT2.Checked = .CheckTech2
            chkCalcT3.Checked = .CheckTech3
            chkCalcStoryline.Checked = .CheckTechStoryline
            chkCalcPirateFaction.Checked = .CheckTechPirate
            chkCalcNavyFaction.Checked = .CheckTechNavy

            ' Blueprint load types
            Select Case .BlueprintType
                Case rbtnCalcAllBPs.Text
                    rbtnCalcAllBPs.Checked = True
                    gbCalcIncludeOwned.Enabled = False
                Case rbtnCalcBPOwned.Text
                    rbtnCalcBPOwned.Checked = True
                    gbCalcIncludeOwned.Enabled = True
            End Select

            cmbCalcBPTypeFilter.Text = .ItemTypeFilter
            txtCalcItemFilter.Text = .TextItemFilter

            chkCalcAutoCalcT2NumBPs.Checked = .CheckAutoCalcNumBPs

            FirstManufacturingGridLoad = False ' Change this now so it will load the grids for all on reset

            chkCalcTaxes.Checked = .CheckIncludeTaxes
            chkCalcFees.Checked = .CheckIncludeBrokersFees

            ' Check wrecked Relics, do not check meta levels or decryptors (NONE)
            chkCalcDecryptor0.CheckState = CType(.CheckDecryptorOptimal, CheckState)
            chkCalcDecryptor1.Checked = .CheckDecryptorNone ' No decryptor
            chkCalcDecryptor2.Checked = .CheckDecryptor06
            chkCalcDecryptor3.Checked = .CheckDecryptor09
            chkCalcDecryptor4.Checked = .CheckDecryptor10
            chkCalcDecryptor5.Checked = .CheckDecryptor11
            chkCalcDecryptor6.Checked = .CheckDecryptor12
            chkCalcDecryptor7.Checked = .CheckDecryptor15
            chkCalcDecryptor8.Checked = .CheckDecryptor18
            chkCalcDecryptor9.Checked = .CheckDecryptor19

            ' Change the name based on the check state
            If chkCalcDecryptor0.CheckState = CheckState.Unchecked Then
                chkCalcDecryptor0.Text = "Optimal"
            ElseIf chkCalcDecryptor0.CheckState = CheckState.Checked Then
                chkCalcDecryptor0.Text = "Optimal IPH"
            ElseIf chkCalcDecryptor0.CheckState = CheckState.Indeterminate Then
                chkCalcDecryptor0.Text = "Optimal Profit"
            End If

            If chkCalcDecryptor0.CheckState <> CheckState.Unchecked Then
                chkCalcDecryptor1.Enabled = False
                chkCalcDecryptor2.Enabled = False
                chkCalcDecryptor3.Enabled = False
                chkCalcDecryptor4.Enabled = False
                chkCalcDecryptor5.Enabled = False
                chkCalcDecryptor6.Enabled = False
                chkCalcDecryptor7.Enabled = False
                chkCalcDecryptor8.Enabled = False
                chkCalcDecryptor9.Enabled = False
            Else
                chkCalcDecryptor1.Enabled = True
                chkCalcDecryptor2.Enabled = True
                chkCalcDecryptor3.Enabled = True
                chkCalcDecryptor4.Enabled = True
                chkCalcDecryptor5.Enabled = True
                chkCalcDecryptor6.Enabled = True
                chkCalcDecryptor7.Enabled = True
                chkCalcDecryptor8.Enabled = True
                chkCalcDecryptor9.Enabled = True
            End If

            chkCalcDecryptorforT2.Checked = .CheckDecryptorUseforT2
            chkCalcDecryptorforT3.Checked = .CheckDecryptorUseforT3

            chkCalcRERelic3.Checked = .CheckRelicIntact
            chkCalcRERelic2.Checked = .CheckRelicMalfunction
            chkCalcRERelic1.Checked = .CheckRelicWrecked

            chkCalcRaceAmarr.Checked = .CheckRaceAmarr
            chkCalcRaceCaldari.Checked = .CheckRaceCaldari
            chkCalcRaceMinmatar.Checked = .CheckRaceMinmatar
            chkCalcRaceGallente.Checked = .CheckRaceGallente
            chkCalcRacePirate.Checked = .CheckRacePirate
            chkCalcRaceOther.Checked = .CheckRaceOther

            chkCalcSmall.Checked = .CheckSmall
            chkCalcMedium.Checked = .CheckMedium
            chkCalcLarge.Checked = .CheckLarge
            chkCalcXL.Checked = .CheckXL

            chkCalcCanBuild.Checked = .CheckOnlyBuild
            chkCalcCanInvent.Checked = .CheckOnlyInvent

            Select Case .PriceCompare
                Case rbtnCalcCompareAll.Text
                    rbtnCalcCompareAll.Checked = True
                Case rbtnCalcCompareBuildBuy.Text
                    rbtnCalcCompareBuildBuy.Checked = True
                Case rbtnCalcCompareComponents.Text
                    rbtnCalcCompareComponents.Checked = True
                Case rbtnCalcCompareRawMats.Text
                    rbtnCalcCompareRawMats.Checked = True
            End Select

            ' Other defaults
            txtCalcTempME.Text = CStr(UserApplicationSettings.DefaultBPME)
            txtCalcTempTE.Text = CStr(UserApplicationSettings.DefaultBPTE)

            chkCalcIncludeT2Owned.Checked = .CheckIncludeT2Owned
            chkCalcIncludeT3Owned.Checked = .CheckIncludeT3Owned

            chkCalcSVRIncludeNull.Checked = .CheckSVRIncludeNull

            txtCalcSVRThreshold.Text = CStr(UserApplicationSettings.IgnoreSVRThresholdValue)
            cmbCalcHistoryRegion.Text = UserApplicationSettings.SVRAveragePriceRegion
            cmbCalcAvgPriceDuration.Text = UserApplicationSettings.SVRAveragePriceDuration

            txtCalcProdLines.Text = CStr(.ProductionLines)
            txtCalcLabLines.Text = CStr(.LaboratoryLines)
            txtCalcRuns.Text = CStr(.Runs)
            txtCalcNumBPs.Text = CStr(.BPRuns)

            ' Time pickers
            chkCalcMaxBuildTimeFilter.Checked = .MaxBuildTimeCheck
            chkCalcMinBuildTimeFilter.Checked = .MinBuildTimeCheck
            tpMaxBuildTimeFilter.Text = .MaxBuildTime
            tpMinBuildTimeFilter.Text = .MinBuildTime

            cmbCalcPriceTrend.Text = .PriceTrend

            ' Thresholds
            chkCalcIPHThreshold.Checked = .IPHThresholdCheck
            txtCalcIPHThreshold.Text = FormatNumber(.IPHThreshold, 2)
            chkCalcVolumeThreshold.Checked = .VolumeThresholdCheck
            txtCalcVolumeThreshold.Text = FormatNumber(.VolumeThreshold, 2)

            ProfitPercentText = "0.0%"
            ProfitText = "0.00"

            Select Case .ProfitThresholdCheck
                Case CheckState.Checked
                    chkCalcProfitThreshold.CheckState = CheckState.Checked
                    txtCalcProfitThreshold.Text = FormatNumber(.ProfitThreshold, 2)
                    ProfitText = txtCalcProfitThreshold.Text
                    chkCalcProfitThreshold.Text = "Profit Threshold"
                    txtCalcProfitThreshold.Enabled = True
                Case CheckState.Unchecked
                    chkCalcProfitThreshold.CheckState = CheckState.Unchecked
                    txtCalcProfitThreshold.Text = FormatNumber(.ProfitThreshold, 2)
                    ProfitText = txtCalcProfitThreshold.Text
                    chkCalcProfitThreshold.Text = "Profit Threshold"
                    txtCalcProfitThreshold.Enabled = False
                Case CheckState.Indeterminate
                    chkCalcProfitThreshold.CheckState = CheckState.Indeterminate
                    txtCalcProfitThreshold.Text = FormatPercent(.ProfitThreshold, 1)
                    ProfitPercentText = txtCalcProfitThreshold.Text
                    chkCalcProfitThreshold.Text = "Profit % Threshold"
                    txtCalcProfitThreshold.Enabled = True
            End Select

            chkCalcPPU.Checked = .CalcPPU

            ListIDIterator = 0

            btnCalcCalculate.Enabled = True
            lstManufacturing.Enabled = True

            If .ColumnSortType = "Ascending" Then
                ManufacturingColumnSortType = SortOrder.Ascending
            Else
                ManufacturingColumnSortType = SortOrder.Descending
            End If

            ManufacturingColumnClicked = .ColumnSort

            AddToShoppingListToolStripMenuItem.Enabled = False ' Don't enable this until they calculate something

        End With

        Call ResetRefresh()
        Call EnableDisableT2T3Options()

    End Sub

    ' Saves all the settings on the screen
    Private Sub btnCalcSaveSettings_Click(sender As System.Object, e As System.EventArgs) Handles btnCalcSaveSettings.Click
        Dim TempSettings As ManufacturingTabSettings = Nothing
        Dim Settings As New ProgramSettings

        ' If they entered an ME/TE value make sure it's ok
        If Trim(txtCalcTempME.Text) <> "" Then
            If Not IsNumeric(txtCalcTempME.Text) Then
                MsgBox("Invalid Temp ME value", vbExclamation, Application.ProductName)
                txtCalcTempME.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtCalcTempTE.Text) <> "" Then
            If Not IsNumeric(txtCalcTempTE.Text) Then
                MsgBox("Invalid Temp TE value", vbExclamation, Application.ProductName)
                txtCalcTempTE.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtCalcSVRThreshold.Text) <> "" Then
            If Not IsNumeric(txtCalcSVRThreshold.Text) Then
                MsgBox("Invalid SVR Threshold value", vbExclamation, Application.ProductName)
                txtCalcSVRThreshold.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtCalcProdLines.Text) <> "" Then
            If Not IsNumeric(txtCalcProdLines.Text) Then
                MsgBox("Invalid Production Lines Value", vbExclamation, Application.ProductName)
                txtCalcProdLines.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtCalcLabLines.Text) <> "" Then
            If Not IsNumeric(txtCalcLabLines.Text) Then
                MsgBox("Invalid Laboratory Lines Value", vbExclamation, Application.ProductName)
                txtCalcLabLines.Focus()
                Exit Sub
            End If
        End If

        If Trim(txtCalcRuns.Text) <> "" Then
            If Not IsNumeric(txtCalcRuns.Text) Then
                MsgBox("Invalid Runs Value", vbExclamation, Application.ProductName)
                txtCalcRuns.Focus()
                Exit Sub
            End If
        End If

        ' Save the column order and width first
        AllSettings.SaveManufacturingTabColumnSettings(UserManufacturingTabColumnSettings)

        With TempSettings
            .CheckBPTypeAmmoCharges = chkCalcAmmo.Checked
            .CheckBPTypeBoosters = chkCalcBoosters.Checked
            .CheckBPTypeComponents = chkCalcComponents.Checked
            .CheckBPTypeDrones = chkCalcDrones.Checked
            .CheckBPTypeModules = chkCalcModules.Checked
            .CheckBPTypeRigs = chkCalcRigs.Checked
            .CheckBPTypeShips = chkCalcShips.Checked
            .CheckBPTypeSubsystems = chkCalcSubsystems.Checked
            .CheckBPTypeStructures = chkCalcStructures.Checked
            .CheckBPTypeMisc = chkCalcMisc.Checked
            .CheckBPTypeDeployables = chkCalcDeployables.Checked
            .CheckBPTypeCelestials = chkCalcCelestials.Checked
            .CheckBPTypeReactions = chkCalcReactions.Checked
            .CheckBPTypeStructureModules = chkCalcStructureModules.Checked
            .CheckBPTypeStationParts = chkCalcStructureRigs.Checked

            .CheckTech1 = chkCalcT1.Checked
            .CheckTech2 = chkCalcT2.Checked
            .CheckTech3 = chkCalcT3.Checked
            .CheckTechStoryline = chkCalcStoryline.Checked
            .CheckTechPirate = chkCalcPirateFaction.Checked
            .CheckTechNavy = chkCalcNavyFaction.Checked

            If CalcComponentsFacility.GetCurrentFacilityProductionType = ProductionType.CapitalComponentManufacturing Then
                .CheckCapitalComponentsFacility = True
            Else
                .CheckCapitalComponentsFacility = False
            End If

            If CalcT3ShipsFacility.GetCurrentFacilityProductionType = ProductionType.T3DestroyerManufacturing Then
                .CheckT3DestroyerFacility = True
            Else
                .CheckT3DestroyerFacility = False
            End If

            .CheckAutoCalcNumBPs = chkCalcAutoCalcT2NumBPs.Checked

            ' Blueprint load types
            If rbtnCalcAllBPs.Checked Then
                .BlueprintType = rbtnCalcAllBPs.Text
            ElseIf rbtnCalcBPOwned.Checked Then
                .BlueprintType = rbtnCalcBPOwned.Text
            End If

            .ItemTypeFilter = cmbCalcBPTypeFilter.Text
            .TextItemFilter = txtCalcItemFilter.Text

            .CheckIncludeTaxes = chkCalcTaxes.Checked
            .CheckIncludeBrokersFees = chkCalcFees.Checked

            .CheckDecryptorOptimal = CInt(chkCalcDecryptor0.CheckState)
            .CheckDecryptorNone = chkCalcDecryptor1.Checked
            .CheckDecryptor06 = chkCalcDecryptor2.Checked
            .CheckDecryptor09 = chkCalcDecryptor3.Checked
            .CheckDecryptor10 = chkCalcDecryptor4.Checked
            .CheckDecryptor11 = chkCalcDecryptor5.Checked
            .CheckDecryptor12 = chkCalcDecryptor6.Checked
            .CheckDecryptor15 = chkCalcDecryptor7.Checked
            .CheckDecryptor18 = chkCalcDecryptor8.Checked
            .CheckDecryptor19 = chkCalcDecryptor9.Checked

            .CheckDecryptorUseforT2 = chkCalcDecryptorforT2.Checked
            .CheckDecryptorUseforT3 = chkCalcDecryptorforT3.Checked

            .CheckRelicIntact = chkCalcRERelic3.Checked
            .CheckRelicMalfunction = chkCalcRERelic2.Checked
            .CheckRelicWrecked = chkCalcRERelic1.Checked

            .CheckRaceAmarr = chkCalcRaceAmarr.Checked
            .CheckRaceCaldari = chkCalcRaceCaldari.Checked
            .CheckRaceMinmatar = chkCalcRaceMinmatar.Checked
            .CheckRaceGallente = chkCalcRaceGallente.Checked
            .CheckRacePirate = chkCalcRacePirate.Checked
            .CheckRaceOther = chkCalcRaceOther.Checked

            .CalcPPU = chkCalcPPU.Checked

            .ColumnSort = ManufacturingColumnClicked
            If ManufacturingColumnSortType = SortOrder.Ascending Then
                .ColumnSortType = "Ascending"
            Else
                .ColumnSortType = "Decending"
            End If

            ' Sort the list based on the saved column, if they change the number of columns below value, then find IPH, if not there, use column 0
            If ManufacturingColumnClicked > lstManufacturing.Columns.Count Then
                ' Find the IPH column
                If UserManufacturingTabColumnSettings.IskperHour <> 0 Then
                    ManufacturingColumnClicked = UserManufacturingTabColumnSettings.IskperHour
                Else
                    ManufacturingColumnClicked = 0 ' Default, will always be there
                End If

            End If

            .ColumnSort = ManufacturingColumnClicked

            If rbtnCalcCompareAll.Checked Then
                .PriceCompare = rbtnCalcCompareAll.Text
            ElseIf rbtnCalcCompareBuildBuy.Checked Then
                .PriceCompare = rbtnCalcCompareBuildBuy.Text
            ElseIf rbtnCalcCompareComponents.Checked Then
                .PriceCompare = rbtnCalcCompareComponents.Text
            ElseIf rbtnCalcCompareRawMats.Checked Then
                .PriceCompare = rbtnCalcCompareRawMats.Text
            End If

            .CheckSmall = chkCalcSmall.Checked
            .CheckMedium = chkCalcMedium.Checked
            .CheckLarge = chkCalcLarge.Checked
            .CheckXL = chkCalcXL.Checked

            .CheckIncludeT2Owned = chkCalcIncludeT2Owned.Checked
            .CheckIncludeT3Owned = chkCalcIncludeT3Owned.Checked

            .CheckSVRIncludeNull = chkCalcSVRIncludeNull.Checked
            .ProductionLines = CInt(txtCalcProdLines.Text)
            .LaboratoryLines = CInt(txtCalcLabLines.Text)
            .Runs = CInt(txtCalcRuns.Text)
            .BPRuns = CInt(txtCalcNumBPs.Text)

            .CheckOnlyBuild = chkCalcCanBuild.Checked
            .CheckOnlyInvent = chkCalcCanInvent.Checked

            .PriceTrend = cmbCalcPriceTrend.Text

            .MaxBuildTimeCheck = chkCalcMaxBuildTimeFilter.Checked
            .MaxBuildTime = tpMaxBuildTimeFilter.Text
            .MinBuildTimeCheck = chkCalcMinBuildTimeFilter.Checked
            .MinBuildTime = tpMinBuildTimeFilter.Text

            .IPHThresholdCheck = chkCalcIPHThreshold.Checked
            .IPHThreshold = CDbl(txtCalcIPHThreshold.Text)
            .VolumeThresholdCheck = chkCalcVolumeThreshold.Checked
            .VolumeThreshold = CDbl(txtCalcVolumeThreshold.Text)

            Select Case chkCalcProfitThreshold.CheckState
                Case CheckState.Checked
                    .ProfitThresholdCheck = CheckState.Checked
                    .ProfitThreshold = CDbl(txtCalcProfitThreshold.Text.Replace("%", ""))
                Case CheckState.Unchecked
                    .ProfitThresholdCheck = CheckState.Unchecked
                    .ProfitThreshold = CDbl(txtCalcProfitThreshold.Text.Replace("%", ""))
                Case CheckState.Indeterminate
                    ' Profit percent
                    .ProfitThresholdCheck = CheckState.Indeterminate
                    .ProfitThreshold = CpctD(txtCalcProfitThreshold.Text)
            End Select

            ' Save these here as well as in settings
            UserApplicationSettings.DefaultBPME = CInt(txtCalcTempME.Text)
            UserApplicationSettings.DefaultBPTE = CInt(txtCalcTempTE.Text)

            UserApplicationSettings.IgnoreSVRThresholdValue = CDbl(txtCalcSVRThreshold.Text)
            UserApplicationSettings.SVRAveragePriceRegion = cmbCalcHistoryRegion.Text
            UserApplicationSettings.SVRAveragePriceDuration = cmbCalcAvgPriceDuration.Text

            Call Settings.SaveApplicationSettings(UserApplicationSettings)

        End With

        ' Save the data in the XML file
        Call Settings.SaveManufacturingSettings(TempSettings)

        ' Save the data to the local variable
        UserManufacturingTabSettings = TempSettings

        MsgBox("Settings Saved", vbInformation, Application.ProductName)

    End Sub

    ' Switches button to calculate
    Public Sub ResetRefresh()
        RefreshCalcData = False
        btnCalcCalculate.Text = "Calculate"
    End Sub

    Structure OptimalDecryptorItem
        Dim ItemTypeID As Long
        Dim ListLocationID As Integer ' The unique number of the item in the list
        Dim CalcType As String ' Raw, Component, or Build/Buy
        Dim CompareValue As Double ' IPH or profit
    End Structure

    ' Displays the results of the options on the screen. If Calculate is true, then it will run the calculations. If not, just a preview of the data
    Private Sub DisplayManufacturingResults(ByVal Calculate As Boolean)
        Dim SQL As String
        Dim readerBPs As SQLiteDataReader
        Dim readerIDs As SQLiteDataReader
        Dim readerArray As SQLiteDataReader

        Dim UpdateTypeIDs As New List(Of Long) ' Full list of TypeID's to update SVR data with, these will have Market IDs
        Dim MarketRegionID As Long
        Dim AveragePriceDays As Integer

        Dim BaseItems As New List(Of ManufacturingItem) ' Holds all the items and their decryptors, relics, meta etc for initial list
        Dim ManufacturingList As New List(Of ManufacturingItem) ' List of all the items we manufactured - may be different than the item list
        Dim FinalItemList As New List(Of ManufacturingItem) ' Final list of data

        Dim InsertItem As New ManufacturingItem

        Dim ManufacturingBlueprint As Blueprint

        Dim BPList As ListViewItem

        Dim i, j As Integer
        Dim BPRecordCount As Integer = 0
        Dim TotalItemCount As Integer = 0
        Dim TempItemType As Integer = 0

        Dim Response As MsgBoxResult

        Dim InventionDecryptors As New DecryptorList

        Dim OrigME As Integer
        Dim OrigTE As Integer

        Dim AddItem As Boolean

        ' For multi-use pos arrays
        Dim ProcessAllMultiUsePOSArrays As Boolean = False
        Dim ArrayName As String = ""
        Dim MultiUsePOSArrays As List(Of IndustryFacility)

        Dim DecryptorUsed As New Decryptor

        ' T2/T3 variables
        Dim RelicName As String = ""
        Dim InputText As String = ""
        Dim DecryptorName As String = ""

        ' BPC stuff
        Dim CopyPricePerSecond As Double = 0
        Dim T1BPCType As String = ""
        Dim T1BPCName As String = ""
        Dim T1BPCMaxRuns As Integer = 0

        ' SVR Threshold
        Dim SVRThresholdValue As Double
        Dim TypeIDCheck As String = ""

        ' Number of blueprints used
        Dim NumberofBlueprints As Integer

        Dim OriginalBPOwnedFlag As Boolean

        ' For the optimal decryptor checking
        Dim OptimalDecryptorItems As New List(Of OptimalDecryptorItem)

        ' Set this now and enable it if they calculate
        AddToShoppingListToolStripMenuItem.Enabled = False

        ' If they entered an ME/TE value make sure it's ok
        If Not CorrectMETE(txtCalcTempME.Text, txtCalcTempTE.Text, txtCalcTempME, txtCalcTempTE) Then
            Exit Sub
        End If

        If Trim(cmbCalcAvgPriceDuration.Text) <> "" Then
            If Not IsNumeric(cmbCalcAvgPriceDuration.Text) Then
                MsgBox("Invalid SVR Average Days. Please select a valid number of days from the combo selection box.", vbExclamation, Application.ProductName)
                cmbCalcAvgPriceDuration.Focus()
                cmbCalcAvgPriceDuration.SelectAll()
                Exit Sub
            End If
        End If

        ' Days can only be between 2 and 365 based on ESI data
        If CInt(cmbCalcAvgPriceDuration.Text) < 2 Or CInt(cmbCalcAvgPriceDuration.Text) > 365 Then
            MsgBox("Averge price updates can only be done for greater than 1 or less than 365 days", vbExclamation, Application.ProductName)
            cmbCalcAvgPriceDuration.Focus()
            cmbCalcAvgPriceDuration.SelectAll()
            Exit Sub
        End If

        If Trim(txtCalcProdLines.Text) <> "" Then
            If Not IsNumeric(txtCalcProdLines.Text) Then
                MsgBox("Invalid Production Lines value", vbExclamation, Application.ProductName)
                txtCalcProdLines.Focus()
                txtCalcProdLines.SelectAll()
                Exit Sub
            End If
        End If

        If Val(txtCalcProdLines.Text) = 0 Then
            MsgBox("You must select a non-zero production lines value.", vbExclamation, Application.ProductName)
            txtCalcProdLines.Focus()
            txtCalcProdLines.SelectAll()
            Exit Sub
        End If

        If Trim(txtCalcNumBPs.Text) <> "" Then
            If Not IsNumeric(txtCalcNumBPs.Text) Then
                MsgBox("Invalid Num BPs value", vbExclamation, Application.ProductName)
                txtCalcNumBPs.Focus()
                txtCalcNumBPs.SelectAll()
                Exit Sub
            End If
        End If

        If Val(txtCalcNumBPs.Text) = 0 Then
            MsgBox("You must select a non-zero Num BPs value.", vbExclamation, Application.ProductName)
            txtCalcNumBPs.Focus()
            txtCalcNumBPs.SelectAll()
            Exit Sub
        End If

        If Trim(txtCalcRuns.Text) <> "" Then
            If Not IsNumeric(txtCalcRuns.Text) Then
                MsgBox("Invalid Runs value", vbExclamation, Application.ProductName)
                txtCalcRuns.Focus()
                txtCalcRuns.SelectAll()
                Exit Sub
            End If
        End If

        If Val(txtCalcRuns.Text) = 0 Then
            MsgBox("You must select a non-zero Runs value.", vbExclamation, Application.ProductName)
            txtCalcRuns.Focus()
            txtCalcRuns.SelectAll()
            Exit Sub
        End If

        If Trim(txtCalcLabLines.Text) <> "" Then
            If Not IsNumeric(txtCalcLabLines.Text) Then
                MsgBox("Invalid Laboratory Lines value", vbExclamation, Application.ProductName)
                txtCalcLabLines.Focus()
                txtCalcLabLines.SelectAll()
                Exit Sub
            End If
        End If

        If Val(txtCalcLabLines.Text) = 0 Then
            MsgBox("You must select a non-zero laboratory lines value.", vbExclamation, Application.ProductName)
            txtCalcLabLines.Focus()
            txtCalcLabLines.SelectAll()
            Exit Sub
        End If

        ' Make sure the build times don't overlap if both checked
        If chkCalcMinBuildTimeFilter.Checked And chkCalcMaxBuildTimeFilter.Checked Then
            If ConvertDHMSTimetoSeconds(tpMinBuildTimeFilter.Text) >= ConvertDHMSTimetoSeconds(tpMaxBuildTimeFilter.Text) Then
                MsgBox("You must select a Min Build time less than the Max Build time selected.", vbExclamation, Application.ProductName)
                chkCalcMinBuildTimeFilter.Focus()
                Exit Sub
            End If
        End If

        If txtCalcSVRThreshold.Text = "" Then
            SVRThresholdValue = Nothing ' Include everything
        Else
            SVRThresholdValue = CDbl(txtCalcSVRThreshold.Text)
        End If

        ' Save the refresh value since everytime we load the facility it will change it
        Dim SavedRefreshValue As Boolean = RefreshCalcData

        ' Make sure they have a facility loaded - if not, load the default for the type
        If Not CalcBaseFacility.GetFacility(ProductionType.Manufacturing).FullyLoaded Then
            CalcBaseFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.Manufacturing)
        End If
        If Not CalcComponentsFacility.GetFacility(ProductionType.ComponentManufacturing).FullyLoaded Then
            CalcComponentsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.ComponentManufacturing)
        End If
        If Not CalcComponentsFacility.GetFacility(ProductionType.CapitalComponentManufacturing).FullyLoaded Then
            CalcComponentsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.CapitalComponentManufacturing)
        End If
        If Not CalcInventionFacility.GetFacility(ProductionType.Invention).FullyLoaded Then
            CalcInventionFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.Invention)
        End If
        If Not CalcCopyFacility.GetFacility(ProductionType.Copying).FullyLoaded Then
            CalcCopyFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.Copying)
        End If
        If Not CalcT3InventionFacility.GetFacility(ProductionType.T3Invention).FullyLoaded Then
            CalcT3InventionFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.T3Invention)
        End If
        If Not CalcSupersFacility.GetFacility(ProductionType.SuperManufacturing).FullyLoaded Then
            CalcSupersFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.SuperManufacturing)
        End If
        If Not CalcCapitalsFacility.GetFacility(ProductionType.CapitalManufacturing).FullyLoaded Then
            CalcCapitalsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.CapitalManufacturing)
        End If
        If Not CalcT3ShipsFacility.GetFacility(ProductionType.T3CruiserManufacturing).FullyLoaded Then
            CalcT3ShipsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.T3CruiserManufacturing)
        End If
        If Not CalcT3ShipsFacility.GetFacility(ProductionType.T3DestroyerManufacturing).FullyLoaded Then
            CalcT3ShipsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.T3DestroyerManufacturing)
        End If
        If Not CalcSubsystemsFacility.GetFacility(ProductionType.SubsystemManufacturing).FullyLoaded Then
            CalcSubsystemsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.SubsystemManufacturing)
        End If
        If Not CalcBoostersFacility.GetFacility(ProductionType.BoosterManufacturing).FullyLoaded Then
            CalcBoostersFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.BoosterManufacturing)
        End If
        If Not CalcReactionsFacility.GetFacility(ProductionType.Reactions).FullyLoaded Then
            CalcReactionsFacility.InitializeFacilities(FacilityView.LimitedControls, ProductionType.Reactions)
        End If

        If Not SavedRefreshValue Then
            Application.UseWaitCursor = True
            Me.Cursor = Cursors.WaitCursor
            Application.DoEvents()

            ' Only cancel if they hit the cancel button
            btnCalcCalculate.Text = "Cancel"

            ' Get the query for the data
            SQL = BuildManufacturingSelectQuery(BPRecordCount, UserInventedBPs)

            If SQL = "" Then
                ' No valid query so just show nothing
                lstManufacturing.Items.Clear()
                FinalManufacturingItemList = Nothing
                GoTo ExitCalc
            End If

            ' Get data
            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            DBCommand.Parameters.AddWithValue("@USERBP_USERID", CStr(SelectedCharacter.ID)) ' need to search for corp ID too
            DBCommand.Parameters.AddWithValue("@USERBP_CORPID", CStr(SelectedCharacter.CharacterCorporation.CorporationID))
            readerBPs = DBCommand.ExecuteReader

            If Not readerBPs.HasRows Then
                ' No data
                lstManufacturing.Items.Clear()
                ' Clear list of data
                FinalManufacturingItemList = Nothing
                GoTo ExitCalc
            End If

            'Me.Cursor = Cursors.WaitCursor
            pnlProgressBar.Minimum = 0
            pnlProgressBar.Maximum = BPRecordCount
            pnlProgressBar.Value = 0
            pnlProgressBar.Visible = True

            ' Reset the record Iterator and list formats
            ListIDIterator = 0
            ListRowFormats = New List(Of RowFormat)

            pnlStatus.Text = "Building List..."

            ' Add the data to the final list, then display into the grid
            While readerBPs.Read
                Application.DoEvents()
                ' If they cancel the calc
                If CancelManufacturingTabCalc Then
                    GoTo ExitCalc
                End If

                ' 0-BP_ID, 1-BLUEPRINT_GROUP, 2-BLUEPRINT_NAME, 3-ITEM_GROUP_ID, 4-ITEM_GROUP, 5-ITEM_CATEGORY_ID, 
                ' 6-ITEM_CATEGORY, 7-ITEM_ID, 8-ITEM_NAME, 9-ME, 10-TE, 11-USERID, 12-ITEM_TYPE, 13-RACE_ID, 14-OWNED, 15-SCANNED 
                ' 16-BP_TYPE, 17-UNIQUE_BP_ITEM_ID, 18-FAVORITE, 19-VOLUME, 20-MARKET_GROUP_ID, 21-ADDITIONAL_COSTS, 
                ' 22-LOCATION_ID, 23-QUANTITY, 24-FLAG_ID, 25-RUNS, 26-IGNORE, 27-TECH_LEVEL
                InsertItem = New ManufacturingItem

                ' Reset
                MultiUsePOSArrays = New List(Of IndustryFacility)
                ProcessAllMultiUsePOSArrays = False
                ArrayName = ""

                ' Save the items before adding
                InsertItem.BPID = CLng(readerBPs.GetValue(0)) ' Hidden
                InsertItem.ItemGroupID = readerBPs.GetInt32(3)
                InsertItem.ItemGroup = readerBPs.GetString(4)
                InsertItem.ItemCategoryID = readerBPs.GetInt32(5)
                InsertItem.ItemCategory = readerBPs.GetString(6)
                InsertItem.ItemTypeID = CLng(readerBPs.GetValue(7))
                InsertItem.ItemName = readerBPs.GetString(8)
                InsertItem.AddlCosts = readerBPs.GetDouble(21)

                ' 1, 2, 14 are T1, T2, T3
                ' 3 is Storyline
                ' 15 is Pirate Faction
                ' 16 is Navy Faction
                TempItemType = CInt(readerBPs.GetValue(12))

                Select Case TempItemType ' For Tech
                    Case 1
                        InsertItem.TechLevel = "T1"
                    Case 2
                        InsertItem.TechLevel = "T2"
                    Case 14
                        InsertItem.TechLevel = "T3"
                    Case 3
                        InsertItem.TechLevel = "Storyline"
                    Case 15
                        InsertItem.TechLevel = "Pirate"
                    Case 16
                        InsertItem.TechLevel = "Navy"
                    Case Else
                        InsertItem.TechLevel = ""
                End Select

                ' Owned flag
                If readerBPs.GetInt32(14) = 0 Then
                    InsertItem.Owned = No
                    OriginalBPOwnedFlag = False
                Else
                    InsertItem.Owned = Yes
                    OriginalBPOwnedFlag = True
                End If

                ' Scanned flag for corp or personal bps
                InsertItem.Scanned = readerBPs.GetInt32(15)

                ' BP Type
                InsertItem.BlueprintType = GetBPType(readerBPs.GetInt32(16))

                ' Save the runs for checking decryptors and relics later
                InsertItem.SavedBPRuns = readerBPs.GetInt32(25)

                ' ME value, either what the entered or in the table
                Select Case TempItemType
                    Case 3, 15, 16
                        ' Storyline, Pirate, and Navy can't be updated
                        InsertItem.BPME = 0
                    Case 2, 14 ' T2 or T3 - either Invented, or BPO
                        If InsertItem.Owned = No Then
                            InsertItem.BPME = BaseT2T3ME
                        Else
                            ' Use what they entered
                            InsertItem.BPME = CInt(readerBPs.GetValue(9))
                        End If
                    Case Else
                        If InsertItem.Owned = No Then
                            ' Use the default
                            InsertItem.BPME = CInt(txtCalcTempME.Text)
                        Else
                            ' Use what they entered
                            InsertItem.BPME = CInt(readerBPs.GetValue(9))
                        End If
                End Select

                ' TE value, either what the entered or in the table
                Select Case TempItemType
                    Case 3, 15, 16
                        ' Storyline, Pirate, and Navy can't be updated
                        InsertItem.BPTE = 0
                    Case 2, 14 ' T2 or T3 - either Invented, or BPO
                        If InsertItem.Owned = No Then
                            InsertItem.BPTE = BaseT2T3TE
                        Else
                            ' Use what they entered
                            InsertItem.BPTE = CInt(readerBPs.GetValue(10))
                        End If
                    Case Else
                        If InsertItem.Owned = No Then
                            ' Use the default
                            InsertItem.BPTE = CInt(txtCalcTempTE.Text)
                        Else
                            ' Use what they entered
                            InsertItem.BPTE = CInt(readerBPs.GetValue(10))
                        End If
                End Select

                ' Default to building/inventing/RE'ing all
                InsertItem.CanBuildBP = True
                InsertItem.CanInvent = True
                InsertItem.CanRE = True

                ' Default prices
                InsertItem.Profit = 0
                InsertItem.ProfitPercent = 0
                InsertItem.IPH = 0

                ' Save the original ME/TE
                OrigME = CInt(InsertItem.BPME)
                OrigTE = CInt(InsertItem.BPTE)

                ' Runs and lines
                InsertItem.Runs = CInt(txtCalcRuns.Text)
                InsertItem.ProductionLines = CInt(txtCalcProdLines.Text)
                InsertItem.LaboratoryLines = CInt(txtCalcLabLines.Text)

                ' Reset all the industry facilities
                InsertItem.ManufacturingFacility = New IndustryFacility
                InsertItem.ComponentManufacturingFacility = New IndustryFacility
                InsertItem.CopyFacility = New IndustryFacility
                InsertItem.InventionFacility = New IndustryFacility

                Dim SelectedIndyType As ProductionType
                Dim TempFacility As New ManufacturingFacility

                ' Set the facility for manufacturing
                If CalcBaseFacility.GetFacility(ProductionType.Manufacturing).FacilityType = FacilityTypes.POS Then
                    ' If this is visible, then look up as a pos, else just look up normally
                    SelectedIndyType = CalcBaseFacility.GetProductionType(InsertItem.ItemGroupID, InsertItem.ItemCategoryID, ManufacturingFacility.ActivityManufacturing)
                    ' See if we will have to add duplicate entries for each type of multi-use array
                    Select Case SelectedIndyType
                        Case ProductionType.POSFuelBlockManufacturing
                            If CalcBaseFacility.GetPOSFuelBlockComboName = "All" Then
                                ProcessAllMultiUsePOSArrays = True
                            Else
                                ProcessAllMultiUsePOSArrays = False
                                ArrayName = GetCalcPOSMultiUseArrayName(CalcBaseFacility.GetPOSFuelBlockComboName)
                            End If
                        Case ProductionType.POSLargeShipManufacturing
                            If CalcBaseFacility.GetPOSLargeShipComboName = "All" Then
                                ProcessAllMultiUsePOSArrays = True
                            Else
                                ProcessAllMultiUsePOSArrays = False
                                ArrayName = GetCalcPOSMultiUseArrayName(CalcBaseFacility.GetPOSLargeShipComboName)
                            End If
                        Case ProductionType.POSModuleManufacturing
                            If CalcBaseFacility.GetPOSModulesComboName = "All" Then
                                ProcessAllMultiUsePOSArrays = True
                            Else
                                ProcessAllMultiUsePOSArrays = False
                                ArrayName = GetCalcPOSMultiUseArrayName(CalcBaseFacility.GetPOSModulesComboName)
                            End If
                        Case Else
                            ProcessAllMultiUsePOSArrays = False
                            ArrayName = ""
                    End Select

                    ' Need to autoselect the pos array by type of blueprint
                    SQL = "SELECT DISTINCT ARRAY_NAME, MATERIAL_MULTIPLIER, TIME_MULTIPLIER FROM ASSEMBLY_ARRAYS "
                    SQL = SQL & "WHERE ACTIVITY_ID = "
                    SQL = SQL & CStr(IndustryActivities.Manufacturing) & " "
                    ' Check groups and categories
                    SQL = SQL & CalcBaseFacility.GetFacilityCatGroupIDSQL(InsertItem.ItemCategoryID, InsertItem.ItemGroupID, IndustryActivities.Manufacturing) & " "
                    If ArrayName <> "" Then
                        SQL = SQL & "AND ARRAY_NAME = '" & ArrayName & "'"
                    End If

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    readerArray = DBCommand.ExecuteReader

                    While readerArray.Read()
                        ' Set the facility
                        InsertItem.ManufacturingFacility = CalcBaseFacility.GetFacility(SelectedIndyType)
                        InsertItem.ManufacturingFacility.FacilityName = readerArray.GetString(0)
                        InsertItem.ManufacturingFacility.MaterialMultiplier = readerArray.GetDouble(1)
                        InsertItem.ManufacturingFacility.TimeMultiplier = readerArray.GetDouble(2)
                        InsertItem.ManufacturingFacility.TaxRate = POSTaxRate

                        ' Add the facility if multiple
                        If ProcessAllMultiUsePOSArrays Then
                            Call MultiUsePOSArrays.Add(InsertItem.ManufacturingFacility)
                        End If
                    End While

                    readerArray.Close()
                Else
                    ' Nothing special, just set it to the current selected facility for this type
                    Dim BuildType As ProductionType = TempFacility.GetProductionType(InsertItem.ItemGroupID, InsertItem.ItemCategoryID, ManufacturingFacility.ActivityManufacturing)
                    Select Case BuildType
                        Case ProductionType.Manufacturing
                            InsertItem.ManufacturingFacility = CalcBaseFacility.GetFacility(BuildType)
                        Case ProductionType.ComponentManufacturing, ProductionType.CapitalComponentManufacturing
                            InsertItem.ManufacturingFacility = CalcComponentsFacility.GetFacility(BuildType)
                        Case ProductionType.BoosterManufacturing
                            InsertItem.ManufacturingFacility = CalcBoostersFacility.GetFacility(BuildType)
                        Case ProductionType.CapitalManufacturing
                            InsertItem.ManufacturingFacility = CalcCapitalsFacility.GetFacility(BuildType)
                        Case ProductionType.Reactions
                            InsertItem.ManufacturingFacility = CalcReactionsFacility.GetFacility(BuildType)
                        Case ProductionType.SubsystemManufacturing
                            InsertItem.ManufacturingFacility = CalcSubsystemsFacility.GetFacility(BuildType)
                        Case ProductionType.SuperManufacturing
                            InsertItem.ManufacturingFacility = CalcSupersFacility.GetFacility(BuildType)
                        Case ProductionType.T3CruiserManufacturing, ProductionType.T3DestroyerManufacturing
                            InsertItem.ManufacturingFacility = CalcT3ShipsFacility.GetFacility(BuildType)
                    End Select
                End If

                ' Set the component, and copy facilities
                InsertItem.ComponentManufacturingFacility = CalcComponentsFacility.GetFacility(ProductionType.ComponentManufacturing)
                InsertItem.CapComponentManufacturingFacility = CalcComponentsFacility.GetFacility(ProductionType.CapitalComponentManufacturing)
                InsertItem.CopyFacility = CalcCopyFacility.GetFacility(ProductionType.Copying)

                ' Now determine how many copies of the base item we need with different data changed
                ' If T1, just select compare types (raw and components)
                ' If T2, first select each decryptor, then select Compare types (raw and components)
                ' If T3, first choose a decryptor, then Relic, then select compare types (raw and components)
                ' Insert each different combination
                If InsertItem.TechLevel = "T2" Or InsertItem.TechLevel = "T3" Then
                    ' For determining the owned blueprints
                    Dim TempDecryptors As New DecryptorList
                    Dim OriginalRelicUsed As String = ""
                    Dim CheckOwnedBP As Boolean = False
                    Dim OriginalBPType As BPType = InsertItem.BlueprintType
                    Dim OriginalDecryptorUsed As Decryptor = TempDecryptors.GetDecryptor(OrigME, OrigTE, InsertItem.SavedBPRuns, CInt(InsertItem.TechLevel.Substring(1)))
                    If InsertItem.TechLevel = "T3" Then
                        OriginalRelicUsed = GetRelicfromInputs(OriginalDecryptorUsed, InsertItem.BPID, InsertItem.SavedBPRuns)
                    End If

                    ' Now add additional records for each decryptor
                    For j = 1 To CalcDecryptorCheckBoxes.Count - 1
                        ' If it's checked or if optimal is checked, add the decryptor
                        If CalcDecryptorCheckBoxes(j).Checked Or chkCalcDecryptor0.Checked Then

                            ' These are all invented BPCs, BPC and BPOs are added separately below
                            InsertItem.BlueprintType = BPType.InventedBPC

                            ' If they are not using for T2 or T3 then only add No Decyrptor and exit for
                            If CalcDecryptorCheckBoxes(j).Text <> None _
                                And ((InsertItem.TechLevel = "T2" And chkCalcDecryptorforT2.Enabled And chkCalcDecryptorforT2.Checked) _
                                Or (InsertItem.TechLevel = "T3" And chkCalcDecryptorforT3.Enabled And chkCalcDecryptorforT3.Checked)) Then

                                ' Select a decryptor
                                DecryptorUsed = InventionDecryptors.GetDecryptor(CDbl(CalcDecryptorCheckBoxes(j).Text.Substring(0, 3)))

                                ' Add decryptor
                                InsertItem.Decryptor = DecryptorUsed
                                InsertItem.Inputs = DecryptorUsed.Name
                                InsertItem.BPME = BaseT2T3ME + InsertItem.Decryptor.MEMod
                                InsertItem.BPTE = BaseT2T3TE + InsertItem.Decryptor.TEMod

                            Else
                                ' Add no decryptor, this is a copy or bpo
                                InsertItem.Decryptor = NoDecryptor
                                InsertItem.Inputs = NoDecryptor.Name
                                InsertItem.BPME = BaseT2T3ME
                                InsertItem.BPTE = BaseT2T3TE
                            End If

                            ' Facilities
                            If InsertItem.TechLevel = "T2" Then
                                InsertItem.InventionFacility = CalcInventionFacility.GetFacility(ProductionType.Invention)
                                InsertItem.CopyFacility = CalcCopyFacility.GetFacility(ProductionType.Copying)
                                InsertItem.InventionFacility.FWUpgradeLevel = CalcComponentsFacility.GetFacility(ProductionType.Invention).FWUpgradeLevel
                            ElseIf InsertItem.TechLevel = "T3" Then
                                InsertItem.InventionFacility = CalcT3InventionFacility.GetFacility(ProductionType.T3Invention)
                                InsertItem.InventionFacility.FWUpgradeLevel = CalcComponentsFacility.GetFacility(ProductionType.T3Invention).FWUpgradeLevel
                                InsertItem.CopyFacility = NoFacility
                            End If

                            InsertItem.CopyFacility.FWUpgradeLevel = CalcComponentsFacility.GetFacility(ProductionType.Copying).FWUpgradeLevel

                            Dim BaseInputs As String = InsertItem.Inputs

                            ' Relics
                            If InsertItem.TechLevel = "T3" Then
                                ' Loop through each relic check box and process for each decryptor
                                For k = 1 To CalcRelicCheckboxes.Count - 1
                                    If CalcRelicCheckboxes(k).Checked Then
                                        InsertItem.Relic = CalcRelicCheckboxes(k).Text
                                        ' Add to the inputs
                                        InsertItem.Inputs = BaseInputs & " - " & InsertItem.Relic
                                        ' Set the owned flag before inserting
                                        CheckOwnedBP = SetItemOwnedFlag(InsertItem, OriginalDecryptorUsed, OriginalRelicUsed, OrigME, OrigTE, OriginalBPOwnedFlag)
                                        If rbtnCalcAllBPs.Checked Or (chkCalcIncludeT3Owned.Checked) Or
                                            (rbtnCalcBPOwned.Checked And CheckOwnedBP) Then
                                            ' Insert the item 
                                            Call InsertItemCalcType(BaseItems, InsertItem, ProcessAllMultiUsePOSArrays, MultiUsePOSArrays, ListRowFormats)
                                        End If
                                    End If
                                Next
                            Else
                                ' No relic for T2
                                InsertItem.Relic = ""
                                ' Set the owned flag before inserting
                                CheckOwnedBP = SetItemOwnedFlag(InsertItem, OriginalDecryptorUsed, OriginalRelicUsed, OrigME, OrigTE, OriginalBPOwnedFlag)
                                If rbtnCalcAllBPs.Checked Or (chkCalcIncludeT2Owned.Checked And UserInventedBPs.Contains(InsertItem.BPID)) Or
                                    (rbtnCalcBPOwned.Checked And CheckOwnedBP) Or rbtnCalcBPFavorites.Checked Then
                                    ' Insert the item 
                                    Call InsertItemCalcType(BaseItems, InsertItem, ProcessAllMultiUsePOSArrays, MultiUsePOSArrays, ListRowFormats)
                                End If
                            End If

                            ' If they don't want to include decryptors, then exit loop after adding none
                            If (InsertItem.TechLevel = "T2" And (chkCalcDecryptorforT2.Enabled = False Or chkCalcDecryptorforT2.Checked = False)) _
                                    Or (InsertItem.TechLevel = "T3" And (chkCalcDecryptorforT3.Enabled = False Or chkCalcDecryptorforT3.Checked = False)) Then
                                Exit For
                            End If
                        End If
                    Next

                    ' Finally, see if the original blueprint was not invented and then add it separately - BPCs and BPOs (should only be T2)
                    If OriginalBPType = BPType.Copy Or OriginalBPType = BPType.Original Then
                        ' Get the original me/te
                        InsertItem.BPME = OrigME
                        InsertItem.BPTE = OrigTE
                        InsertItem.Owned = Yes
                        InsertItem.Inputs = Unknown
                        InsertItem.BlueprintType = OriginalBPType

                        ' Insert the item 
                        Call InsertItemCalcType(BaseItems, InsertItem, ProcessAllMultiUsePOSArrays, MultiUsePOSArrays, ListRowFormats)
                    End If

                Else ' All T1 and others
                    InsertItem.Inputs = None
                    InsertItem.Relic = ""
                    InsertItem.Decryptor = NoDecryptor

                    InsertItem.InventionFacility = NoFacility
                    InsertItem.CopyFacility = NoFacility

                    ' Insert the items based on compare types
                    Call InsertItemCalcType(BaseItems, InsertItem, ProcessAllMultiUsePOSArrays, MultiUsePOSArrays, ListRowFormats)
                End If

                ' For each record, update the progress bar
                Call IncrementToolStripProgressBar(pnlProgressBar)

            End While

            ' Set the formats
            Call lstManufacturing.SetRowFormats(ListRowFormats)
            Application.DoEvents()

            readerBPs.Close()
            readerBPs = Nothing
            DBCommand = Nothing

            TotalItemCount = BaseItems.Count

            ' *** Calculate ***
            ' Got all the data, now see if they want to calculate prices
            If Calculate Then
                If TotalItemCount > 1000 Then
                    ' Make sure they know this will take a bit to run - unless this is fairly quick
                    Response = MsgBox("This may take some time to complete. Do you want to continue?", vbYesNo, Me.Text)

                    If Response = vbNo Then
                        ' Just display the results of the query
                        GoTo DisplayResults
                    End If
                End If

                ListIDIterator = 0 ' Reset the iterator for new list
                ' Reset the format list and recalc
                ListRowFormats = New List(Of RowFormat)

                ' Disable all the controls individulally so we can use cancel button
                btnCalcPreview.Enabled = False
                btnCalcReset.Enabled = False
                btnCalcSelectColumns.Enabled = False
                btnCalcSaveSettings.Enabled = False
                btnCalcExportList.Enabled = False
                gbCalcBPSelect.Enabled = False
                gbCalcBPTech.Enabled = False
                gbCalcCompareType.Enabled = False
                gbCalcMarketFilters.Enabled = False
                gbCalcFilter.Enabled = False
                gbCalcIgnoreinCalcs.Enabled = False
                gbCalcIncludeOwned.Enabled = False
                gbCalcInvention.Enabled = False
                gbCalcProdLines.Enabled = False
                gbCalcRelics.Enabled = False
                gbCalcTextColors.Enabled = False
                gbCalcTextFilter.Enabled = False
                lstManufacturing.Enabled = False
                tabCalcFacilities.Enabled = False

                If Not UserApplicationSettings.DisableSVR Then

                    Dim MH As New MarketPriceInterface(pnlProgressBar)

                    ' First thing we want to do is update the manufactured item prices
                    pnlStatus.Text = "Updating Market History..."
                    pnlProgressBar.Visible = False
                    Application.DoEvents()

                    ' First find out which of the typeIDs in BaseItems have MarketID's
                    For i = 0 To BaseItems.Count - 1
                        TypeIDCheck = TypeIDCheck & BaseItems(i).ItemTypeID & ","
                    Next

                    ' Format string
                    TypeIDCheck = "(" & TypeIDCheck.Substring(0, Len(TypeIDCheck) - 1) & ")"
                    SQL = "SELECT typeID FROM INVENTORY_TYPES WHERE typeID IN " & TypeIDCheck & " AND marketGroupID IS NOT NULL"
                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    readerIDs = DBCommand.ExecuteReader

                    ' Now add these to the list
                    While readerIDs.Read()
                        If Not UpdateTypeIDs.Contains(readerIDs.GetInt64(0)) Then
                            UpdateTypeIDs.Add(readerIDs.GetInt64(0))
                        End If
                    End While
                    readerIDs.Close()

                    AveragePriceDays = CInt(cmbCalcAvgPriceDuration.Text)
                    ' Get the region ID
                    MarketRegionID = GetRegionID(cmbCalcHistoryRegion.Text)

                    If MarketRegionID = 0 Then
                        MarketRegionID = TheForgeTypeID ' The Forge as default
                        cmbCalcHistoryRegion.Text = "The Forge"
                    End If

                    ' Update the prices
                    Dim timecheck As Date = Now
                    If Not MH.UpdateESIPriceHistory(UpdateTypeIDs, MarketRegionID) Then
                        Call MsgBox("Price update timed out for some items. Please try again.", vbInformation, Application.ProductName)
                    End If

                End If

                pnlStatus.Text = "Calculating..."
                pnlProgressBar.Minimum = 0
                pnlProgressBar.Maximum = TotalItemCount
                pnlProgressBar.Value = 0
                pnlProgressBar.Visible = True

                Application.DoEvents()

                ' Loop through the item list and calculate data
                For i = 0 To BaseItems.Count - 1

                    Application.DoEvents()

                    InsertItem = BaseItems(i)

                    ' If they cancel the calc
                    If CancelManufacturingTabCalc Then
                        GoTo ExitCalc
                    End If

                    ' Set the number of BPs
                    With InsertItem
                        If (.TechLevel = "T2" Or .TechLevel = "T3") And chkCalcAutoCalcT2NumBPs.Checked = True And (.BlueprintType = BPType.InventedBPC Or .BlueprintType = BPType.NotOwned) Then
                            ' For T3 or if they have calc checked, we will never have a BPO so determine the number of BPs
                            NumberofBlueprints = GetUsedNumBPs(.BPID, CInt(.TechLevel.Substring(1, 1)), .Runs, .ProductionLines, .NumBPs, .Decryptor.RunMod)
                        Else
                            NumberofBlueprints = CInt(txtCalcNumBPs.Text)
                        End If
                    End With

                    ' Construct the BP
                    ManufacturingBlueprint = New Blueprint(InsertItem.BPID, CInt(txtCalcRuns.Text), InsertItem.BPME, InsertItem.BPTE,
                                   NumberofBlueprints, CInt(txtCalcProdLines.Text), SelectedCharacter,
                                   UserApplicationSettings, rbtnCalcCompareBuildBuy.Checked, InsertItem.AddlCosts, InsertItem.ManufacturingFacility,
                                  InsertItem.ComponentManufacturingFacility, InsertItem.CapComponentManufacturingFacility)

                    ' Set the T2 and T3 inputs if necessary
                    If ((InsertItem.TechLevel = "T2" Or InsertItem.TechLevel = "T3") And InsertItem.BlueprintType = BPType.InventedBPC) And chkCalcIgnoreInvention.Checked = False Then

                        ' Strip off the relic if in here for the decryptor
                        If InsertItem.Inputs.Contains("-") Then
                            InputText = InsertItem.Inputs.Substring(0, InStr(InsertItem.Inputs, "-") - 2)
                        Else
                            InputText = InsertItem.Inputs
                        End If

                        If InputText = None Then
                            SelectedDecryptor = NoDecryptor
                        Else ' A decryptor is set
                            SelectedDecryptor = InventionDecryptors.GetDecryptor(InputText)
                        End If

                        ' Construct the T2/T3 BP
                        Call ManufacturingBlueprint.InventBlueprint(CInt(txtCalcLabLines.Text), SelectedDecryptor, InsertItem.InventionFacility,
                                                               InsertItem.CopyFacility, GetInventItemTypeID(InsertItem.BPID, InsertItem.Relic))

                    End If

                    ' Build the blueprint(s)
                    Call ManufacturingBlueprint.BuildItems(chkCalcTaxes.Checked, chkCalcFees.Checked, False, chkCalcIgnoreMinerals.Checked, chkCalcIgnoreT1Item.Checked)

                    ' If checked, Add the values to the array only if we can Build, Invent, or RE it
                    AddItem = True

                    ' User can Build
                    If chkCalcCanBuild.Checked And Not ManufacturingBlueprint.UserCanBuildBlueprint Then
                        AddItem = False
                    End If

                    ' User can Invent
                    If chkCalcCanInvent.Checked And chkCalcCanInvent.Enabled And Not ManufacturingBlueprint.UserCanInventRE And (ManufacturingBlueprint.GetTechLevel = 2 Or ManufacturingBlueprint.GetTechLevel = 3) Then
                        AddItem = False
                    End If

                    ' Adjust the item with calculations
                    If AddItem Then
                        Application.DoEvents()
                        ' Add data that will the same for all options (need to move more from the bottom but have to test)
                        InsertItem.CanBuildBP = ManufacturingBlueprint.UserCanBuildBlueprint
                        InsertItem.CanInvent = ManufacturingBlueprint.UserCanInventRE
                        InsertItem.CanRE = ManufacturingBlueprint.UserCanInventRE
                        ' Trend data
                        InsertItem.PriceTrend = CalculatePriceTrend(InsertItem.ItemTypeID, MarketRegionID, CInt(cmbCalcAvgPriceDuration.Text))
                        InsertItem.ItemMarketPrice = ManufacturingBlueprint.GetItemMarketPrice

                        ' Add all the volume, items on hand, etc here since they won't change
                        InsertItem.TotalItemsSold = CalculateTotalItemsSold(InsertItem.ItemTypeID, MarketRegionID, CInt(cmbCalcAvgPriceDuration.Text))
                        InsertItem.TotalOrdersFilled = CalculateTotalOrdersFilled(InsertItem.ItemTypeID, MarketRegionID, CInt(cmbCalcAvgPriceDuration.Text))
                        InsertItem.AvgItemsperOrder = CDbl(IIf(InsertItem.TotalOrdersFilled = 0, 0, InsertItem.TotalItemsSold / InsertItem.TotalOrdersFilled))
                        Call GetCurrentOrders(InsertItem.ItemTypeID, MarketRegionID, InsertItem.CurrentBuyOrders, InsertItem.CurrentSellOrders)

                        InsertItem.ItemsinStock = GetTotalItemsinStock(InsertItem.ItemTypeID)
                        InsertItem.ItemsinProduction = GetTotalItemsinProduction(InsertItem.ItemTypeID)

                        ' Get the output data
                        If rbtnCalcCompareAll.Checked Then
                            ' Need to add a record for each of the three types

                            ' *** For components, only add if it has buildable components
                            If ManufacturingBlueprint.HasComponents Then
                                ' Components first
                                InsertItem.ProfitPercent = ManufacturingBlueprint.GetTotalComponentProfitPercent
                                InsertItem.Profit = ManufacturingBlueprint.GetTotalComponentProfit
                                InsertItem.IPH = ManufacturingBlueprint.GetTotalIskperHourComponents
                                InsertItem.CalcType = "Components"
                                InsertItem.SVR = GetItemSVR(InsertItem.ItemTypeID, MarketRegionID, AveragePriceDays, ManufacturingBlueprint.GetProductionTime, ManufacturingBlueprint.GetTotalUnits)
                                If InsertItem.SVR = "-" Then
                                    InsertItem.SVRxIPH = "0.00"
                                Else
                                    InsertItem.SVRxIPH = FormatNumber(CType(InsertItem.SVR, Double) * InsertItem.IPH, 2)
                                End If
                                InsertItem.TotalCost = ManufacturingBlueprint.GetTotalComponentCost
                                InsertItem.Taxes = ManufacturingBlueprint.GetSalesTaxes
                                InsertItem.BrokerFees = ManufacturingBlueprint.GetSalesBrokerFees
                                InsertItem.SingleInventedBPCRunsperBPC = ManufacturingBlueprint.GetSingleInventedBPCRuns
                                InsertItem.BaseJobCost = ManufacturingBlueprint.GetBaseJobCost
                                InsertItem.JobFee = ManufacturingBlueprint.GetJobFee
                                InsertItem.NumBPs = ManufacturingBlueprint.GetUsedNumBPs
                                InsertItem.InventionChance = ManufacturingBlueprint.GetInventionChance
                                InsertItem.Race = GetRace(ManufacturingBlueprint.GetRaceID)
                                InsertItem.VolumeperItem = ManufacturingBlueprint.GetItemVolume
                                InsertItem.TotalVolume = ManufacturingBlueprint.GetTotalItemVolume

                                If chkCalcPPU.Checked Then
                                    InsertItem.DivideUnits = CInt(ManufacturingBlueprint.GetTotalUnits)
                                    InsertItem.PortionSize = 1
                                Else
                                    InsertItem.DivideUnits = 1
                                    InsertItem.PortionSize = CInt(ManufacturingBlueprint.GetTotalUnits)
                                End If

                                InsertItem.BPProductionTime = FormatIPHTime(ManufacturingBlueprint.GetProductionTime / InsertItem.DivideUnits)
                                InsertItem.TotalProductionTime = FormatIPHTime(ManufacturingBlueprint.GetProductionTime / InsertItem.DivideUnits) ' Total production time for components only is always the bp production time
                                InsertItem.CopyTime = FormatIPHTime(ManufacturingBlueprint.GetCopyTime / InsertItem.DivideUnits)
                                InsertItem.InventionTime = FormatIPHTime(ManufacturingBlueprint.GetInventionTime / InsertItem.DivideUnits)

                                If ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 Or ManufacturingBlueprint.GetTechLevel = BPTechLevel.T3 Then
                                    InsertItem.InventionCost = ManufacturingBlueprint.GetInventionCost
                                Else
                                    InsertItem.InventionCost = 0
                                End If

                                If ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 Then
                                    InsertItem.CopyCost = ManufacturingBlueprint.GetCopyCost
                                Else
                                    InsertItem.CopyCost = 0
                                End If

                                ' Usage
                                InsertItem.ManufacturingFacilityUsage = ManufacturingBlueprint.GetManufacturingFacilityUsage
                                ' Don't build components in this calculation
                                InsertItem.ComponentManufacturingFacilityUsage = 0
                                InsertItem.CapComponentManufacturingFacilityUsage = 0
                                InsertItem.CopyFacilityUsage = ManufacturingBlueprint.GetCopyUsage
                                InsertItem.InventionFacilityUsage = ManufacturingBlueprint.GetInventionUsage
                                ' Save the bp
                                InsertItem.Blueprint = ManufacturingBlueprint

                                ' Insert Components Item
                                Call InsertManufacturingItem(InsertItem, SVRThresholdValue, chkCalcSVRIncludeNull.Checked, ManufacturingList, ListRowFormats)

                                ' Insert the item for decryptor compare
                                Call InsertDecryptorforOptimalCompare(ManufacturingBlueprint, InsertItem.CalcType, InsertItem.ListID, OptimalDecryptorItems)

                            End If

                            ' *** Raw Mats - always add
                            InsertItem.ProfitPercent = ManufacturingBlueprint.GetTotalRawProfitPercent
                            InsertItem.Profit = ManufacturingBlueprint.GetTotalRawProfit
                            InsertItem.IPH = ManufacturingBlueprint.GetTotalIskperHourRaw
                            InsertItem.CalcType = "Raw Materials"
                            InsertItem.SVR = GetItemSVR(InsertItem.ItemTypeID, MarketRegionID, AveragePriceDays, ManufacturingBlueprint.GetTotalProductionTime, ManufacturingBlueprint.GetTotalUnits)
                            If InsertItem.SVR = "-" Then
                                InsertItem.SVRxIPH = "0.00"
                            Else
                                InsertItem.SVRxIPH = FormatNumber(CType(InsertItem.SVR, Double) * InsertItem.IPH, 2)
                            End If
                            InsertItem.TotalCost = ManufacturingBlueprint.GetTotalRawCost
                            InsertItem.Taxes = ManufacturingBlueprint.GetSalesTaxes
                            InsertItem.BrokerFees = ManufacturingBlueprint.GetSalesBrokerFees
                            InsertItem.SingleInventedBPCRunsperBPC = ManufacturingBlueprint.GetSingleInventedBPCRuns
                            InsertItem.BaseJobCost = ManufacturingBlueprint.GetBaseJobCost
                            InsertItem.JobFee = ManufacturingBlueprint.GetJobFee
                            InsertItem.NumBPs = ManufacturingBlueprint.GetUsedNumBPs
                            InsertItem.InventionChance = ManufacturingBlueprint.GetInventionChance
                            InsertItem.Race = GetRace(ManufacturingBlueprint.GetRaceID)
                            InsertItem.VolumeperItem = ManufacturingBlueprint.GetItemVolume
                            InsertItem.TotalVolume = ManufacturingBlueprint.GetTotalItemVolume

                            If chkCalcPPU.Checked Then
                                InsertItem.DivideUnits = CInt(ManufacturingBlueprint.GetTotalUnits)
                                InsertItem.PortionSize = 1
                            Else
                                InsertItem.DivideUnits = 1
                                InsertItem.PortionSize = CInt(ManufacturingBlueprint.GetTotalUnits)
                            End If

                            InsertItem.BPProductionTime = FormatIPHTime(ManufacturingBlueprint.GetProductionTime / InsertItem.DivideUnits)
                            InsertItem.TotalProductionTime = FormatIPHTime(ManufacturingBlueprint.GetTotalProductionTime / InsertItem.DivideUnits)
                            InsertItem.CopyTime = FormatIPHTime(ManufacturingBlueprint.GetCopyTime / InsertItem.DivideUnits)
                            InsertItem.InventionTime = FormatIPHTime(ManufacturingBlueprint.GetInventionTime / InsertItem.DivideUnits)

                            If (ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 Or ManufacturingBlueprint.GetTechLevel = BPTechLevel.T3) And InsertItem.BlueprintType <> BPType.Original Then
                                InsertItem.InventionCost = ManufacturingBlueprint.GetInventionCost
                            Else
                                InsertItem.InventionCost = 0
                            End If

                            If ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 And InsertItem.BlueprintType <> BPType.Original Then
                                InsertItem.CopyCost = ManufacturingBlueprint.GetCopyCost
                            Else
                                InsertItem.CopyCost = 0
                            End If

                            ' Usage
                            InsertItem.ManufacturingFacilityUsage = ManufacturingBlueprint.GetManufacturingFacilityUsage
                            InsertItem.ComponentManufacturingFacilityUsage = ManufacturingBlueprint.GetComponentFacilityUsage
                            InsertItem.CapComponentManufacturingFacilityUsage = ManufacturingBlueprint.GetCapComponentFacilityUsage
                            InsertItem.CopyFacilityUsage = ManufacturingBlueprint.GetCopyUsage
                            InsertItem.InventionFacilityUsage = ManufacturingBlueprint.GetInventionUsage

                            ' Save the bp
                            InsertItem.Blueprint = ManufacturingBlueprint

                            ' Insert Raw Mats item
                            Call InsertManufacturingItem(InsertItem, SVRThresholdValue, chkCalcSVRIncludeNull.Checked, ManufacturingList, ListRowFormats)

                            ' Insert the item for decryptor compare
                            Call InsertDecryptorforOptimalCompare(ManufacturingBlueprint, InsertItem.CalcType, InsertItem.ListID, OptimalDecryptorItems)

                            ' *** For Build/Buy we need to construct a new BP and add that
                            ' Construct the BP
                            ManufacturingBlueprint = New Blueprint(InsertItem.BPID, CInt(txtCalcRuns.Text), InsertItem.BPME, InsertItem.BPTE,
                                                        NumberofBlueprints, CInt(txtCalcProdLines.Text), SelectedCharacter,
                                                        UserApplicationSettings, True, InsertItem.AddlCosts, InsertItem.ManufacturingFacility,
                                                        InsertItem.ComponentManufacturingFacility, InsertItem.CapComponentManufacturingFacility)

                            If (InsertItem.TechLevel = "T2" Or InsertItem.TechLevel = "T3") And chkCalcIgnoreInvention.Checked = False Then
                                ' Construct the T2/T3 BP
                                ManufacturingBlueprint.InventBlueprint(CInt(txtCalcLabLines.Text), SelectedDecryptor, InsertItem.InventionFacility,
                                                                       InsertItem.CopyFacility, GetInventItemTypeID(InsertItem.BPID, InsertItem.Relic))

                            End If

                            ' Get the list of materials
                            Call ManufacturingBlueprint.BuildItems(chkCalcTaxes.Checked, chkCalcFees.Checked, False, chkCalcIgnoreMinerals.Checked, chkCalcIgnoreT1Item.Checked)

                            ' Build/Buy (add only if it has components we build)
                            If ManufacturingBlueprint.HasComponents Then
                                InsertItem.ProfitPercent = ManufacturingBlueprint.GetTotalRawProfitPercent
                                InsertItem.Profit = ManufacturingBlueprint.GetTotalRawProfit
                                InsertItem.IPH = ManufacturingBlueprint.GetTotalIskperHourRaw
                                InsertItem.CalcType = "Build/Buy"
                                InsertItem.SVR = GetItemSVR(InsertItem.ItemTypeID, MarketRegionID, AveragePriceDays, ManufacturingBlueprint.GetTotalProductionTime, ManufacturingBlueprint.GetTotalUnits)
                                If InsertItem.SVR = "-" Then
                                    InsertItem.SVRxIPH = "0.00"
                                Else
                                    InsertItem.SVRxIPH = FormatNumber(CType(InsertItem.SVR, Double) * InsertItem.IPH, 2)
                                End If
                                InsertItem.TotalCost = ManufacturingBlueprint.GetTotalRawCost
                                InsertItem.Taxes = ManufacturingBlueprint.GetSalesTaxes
                                InsertItem.BrokerFees = ManufacturingBlueprint.GetSalesBrokerFees
                                InsertItem.SingleInventedBPCRunsperBPC = ManufacturingBlueprint.GetSingleInventedBPCRuns
                                InsertItem.BaseJobCost = ManufacturingBlueprint.GetBaseJobCost
                                InsertItem.JobFee = ManufacturingBlueprint.GetJobFee
                                InsertItem.NumBPs = ManufacturingBlueprint.GetUsedNumBPs
                                InsertItem.InventionChance = ManufacturingBlueprint.GetInventionChance
                                InsertItem.Race = GetRace(ManufacturingBlueprint.GetRaceID)
                                InsertItem.VolumeperItem = ManufacturingBlueprint.GetItemVolume
                                InsertItem.TotalVolume = ManufacturingBlueprint.GetTotalItemVolume

                                If chkCalcPPU.Checked Then
                                    InsertItem.DivideUnits = CInt(ManufacturingBlueprint.GetTotalUnits)
                                    InsertItem.PortionSize = 1
                                Else
                                    InsertItem.DivideUnits = 1
                                    InsertItem.PortionSize = CInt(ManufacturingBlueprint.GetTotalUnits)
                                End If

                                InsertItem.BPProductionTime = FormatIPHTime(ManufacturingBlueprint.GetProductionTime / InsertItem.DivideUnits)
                                InsertItem.TotalProductionTime = FormatIPHTime(ManufacturingBlueprint.GetTotalProductionTime / InsertItem.DivideUnits)
                                InsertItem.CopyTime = FormatIPHTime(ManufacturingBlueprint.GetCopyTime / InsertItem.DivideUnits)
                                InsertItem.InventionTime = FormatIPHTime(ManufacturingBlueprint.GetInventionTime / InsertItem.DivideUnits)

                                If (ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 Or ManufacturingBlueprint.GetTechLevel = BPTechLevel.T3) And InsertItem.BlueprintType <> BPType.Original Then
                                    InsertItem.InventionCost = ManufacturingBlueprint.GetInventionCost
                                Else
                                    InsertItem.InventionCost = 0
                                End If

                                If ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 And InsertItem.BlueprintType <> BPType.Original Then
                                    InsertItem.CopyCost = ManufacturingBlueprint.GetCopyCost
                                Else
                                    InsertItem.CopyCost = 0
                                End If

                                ' Usage
                                InsertItem.ManufacturingFacilityUsage = ManufacturingBlueprint.GetManufacturingFacilityUsage
                                InsertItem.ComponentManufacturingFacilityUsage = ManufacturingBlueprint.GetComponentFacilityUsage
                                InsertItem.CapComponentManufacturingFacilityUsage = ManufacturingBlueprint.GetCapComponentFacilityUsage
                                InsertItem.CopyFacilityUsage = ManufacturingBlueprint.GetCopyUsage
                                InsertItem.InventionFacilityUsage = ManufacturingBlueprint.GetInventionUsage

                                ' Save the bp
                                InsertItem.Blueprint = ManufacturingBlueprint

                                ' Insert Build/Buy item
                                Call InsertManufacturingItem(InsertItem, SVRThresholdValue, chkCalcSVRIncludeNull.Checked, ManufacturingList, ListRowFormats)

                                ' Insert the item for decryptor compare
                                Call InsertDecryptorforOptimalCompare(ManufacturingBlueprint, InsertItem.CalcType, InsertItem.ListID, OptimalDecryptorItems)

                            End If
                        Else

                            ' Just look at each one individually
                            If rbtnCalcCompareComponents.Checked Then
                                ' Use the Component values
                                InsertItem.ProfitPercent = ManufacturingBlueprint.GetTotalComponentProfitPercent
                                InsertItem.Profit = ManufacturingBlueprint.GetTotalComponentProfit
                                InsertItem.IPH = ManufacturingBlueprint.GetTotalIskperHourComponents
                                InsertItem.CalcType = "Components"
                                InsertItem.SVR = GetItemSVR(InsertItem.ItemTypeID, MarketRegionID, AveragePriceDays, ManufacturingBlueprint.GetProductionTime, ManufacturingBlueprint.GetTotalUnits)
                                If InsertItem.SVR = "-" Then
                                    InsertItem.SVRxIPH = "0.00"
                                Else
                                    InsertItem.SVRxIPH = FormatNumber(CType(InsertItem.SVR, Double) * InsertItem.IPH, 2)
                                End If
                                InsertItem.TotalCost = ManufacturingBlueprint.GetTotalComponentCost
                            ElseIf rbtnCalcCompareRawMats.Checked Then
                                ' Use the Raw values 
                                InsertItem.ProfitPercent = ManufacturingBlueprint.GetTotalRawProfitPercent
                                InsertItem.Profit = ManufacturingBlueprint.GetTotalRawProfit
                                InsertItem.IPH = ManufacturingBlueprint.GetTotalIskperHourRaw
                                InsertItem.CalcType = "Raw Materials"
                                InsertItem.SVR = GetItemSVR(InsertItem.ItemTypeID, MarketRegionID, AveragePriceDays, ManufacturingBlueprint.GetTotalProductionTime, ManufacturingBlueprint.GetTotalUnits)
                                If InsertItem.SVR = "-" Then
                                    InsertItem.SVRxIPH = "0.00"
                                Else
                                    InsertItem.SVRxIPH = FormatNumber(CType(InsertItem.SVR, Double) * InsertItem.IPH, 2)
                                End If
                                InsertItem.TotalCost = ManufacturingBlueprint.GetTotalRawCost
                            ElseIf rbtnCalcCompareBuildBuy.Checked Then
                                ' Use the Build/Buy best rate values (the blueprint was set to get these values above)
                                InsertItem.ProfitPercent = ManufacturingBlueprint.GetTotalRawProfitPercent
                                InsertItem.Profit = ManufacturingBlueprint.GetTotalRawProfit
                                InsertItem.IPH = ManufacturingBlueprint.GetTotalIskperHourRaw
                                InsertItem.CalcType = "Build/Buy"
                                InsertItem.SVR = GetItemSVR(InsertItem.ItemTypeID, MarketRegionID, AveragePriceDays, ManufacturingBlueprint.GetTotalProductionTime, ManufacturingBlueprint.GetTotalUnits)
                                If InsertItem.SVR = "-" Then
                                    InsertItem.SVRxIPH = "0.00"
                                Else
                                    InsertItem.SVRxIPH = FormatNumber(CType(InsertItem.SVR, Double) * InsertItem.IPH, 2)
                                End If
                                InsertItem.TotalCost = ManufacturingBlueprint.GetTotalRawCost
                            End If

                            InsertItem.ManufacturingFacilityUsage = ManufacturingBlueprint.GetManufacturingFacilityUsage
                            InsertItem.Taxes = ManufacturingBlueprint.GetSalesTaxes
                            InsertItem.BrokerFees = ManufacturingBlueprint.GetSalesBrokerFees
                            InsertItem.SingleInventedBPCRunsperBPC = ManufacturingBlueprint.GetSingleInventedBPCRuns
                            InsertItem.BaseJobCost = ManufacturingBlueprint.GetBaseJobCost
                            InsertItem.JobFee = ManufacturingBlueprint.GetJobFee
                            InsertItem.NumBPs = ManufacturingBlueprint.GetUsedNumBPs
                            InsertItem.InventionChance = ManufacturingBlueprint.GetInventionChance
                            InsertItem.Race = GetRace(ManufacturingBlueprint.GetRaceID)
                            InsertItem.VolumeperItem = ManufacturingBlueprint.GetItemVolume
                            InsertItem.TotalVolume = ManufacturingBlueprint.GetTotalItemVolume

                            If chkCalcPPU.Checked Then
                                InsertItem.DivideUnits = CInt(ManufacturingBlueprint.GetTotalUnits)
                                InsertItem.PortionSize = 1
                            Else
                                InsertItem.DivideUnits = 1
                                InsertItem.PortionSize = CInt(ManufacturingBlueprint.GetTotalUnits)
                            End If

                            InsertItem.BPProductionTime = FormatIPHTime(ManufacturingBlueprint.GetProductionTime / InsertItem.DivideUnits)
                            If rbtnCalcCompareComponents.Checked Then
                                ' Total production time for components only is always the bp production time
                                InsertItem.TotalProductionTime = FormatIPHTime(ManufacturingBlueprint.GetProductionTime / InsertItem.DivideUnits)
                            Else
                                InsertItem.TotalProductionTime = FormatIPHTime(ManufacturingBlueprint.GetTotalProductionTime / InsertItem.DivideUnits)
                            End If

                            InsertItem.CopyTime = FormatIPHTime(ManufacturingBlueprint.GetCopyTime / InsertItem.DivideUnits)
                            InsertItem.InventionTime = FormatIPHTime(ManufacturingBlueprint.GetInventionTime / InsertItem.DivideUnits)

                            If (ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 Or ManufacturingBlueprint.GetTechLevel = BPTechLevel.T3) And InsertItem.BlueprintType <> BPType.Original Then
                                InsertItem.InventionCost = ManufacturingBlueprint.GetInventionCost
                            Else
                                InsertItem.InventionCost = 0
                            End If

                            If ManufacturingBlueprint.GetTechLevel = BPTechLevel.T2 And InsertItem.BlueprintType <> BPType.Original Then
                                InsertItem.CopyCost = ManufacturingBlueprint.GetCopyCost
                            Else
                                InsertItem.CopyCost = 0
                            End If

                            ' Usage
                            InsertItem.ManufacturingFacilityUsage = ManufacturingBlueprint.GetManufacturingFacilityUsage
                            InsertItem.ComponentManufacturingFacilityUsage = ManufacturingBlueprint.GetComponentFacilityUsage
                            InsertItem.CapComponentManufacturingFacilityUsage = ManufacturingBlueprint.GetCapComponentFacilityUsage
                            InsertItem.CopyFacilityUsage = ManufacturingBlueprint.GetCopyUsage
                            InsertItem.InventionFacilityUsage = ManufacturingBlueprint.GetInventionUsage

                            ' Save the bp
                            InsertItem.Blueprint = ManufacturingBlueprint

                            ' Insert the chosen item
                            Call InsertManufacturingItem(InsertItem, SVRThresholdValue, chkCalcSVRIncludeNull.Checked, ManufacturingList, ListRowFormats)

                            ' Insert the item for decryptor compare
                            Call InsertDecryptorforOptimalCompare(ManufacturingBlueprint, InsertItem.CalcType, InsertItem.ListID, OptimalDecryptorItems)

                        End If

                    End If

                    ' For each record, update the progress bar
                    Call IncrementToolStripProgressBar(pnlProgressBar)

                Next

                ' Done processing the blueprints
                pnlProgressBar.Value = 0
                pnlProgressBar.Visible = False
                'Me.Cursor = Cursors.Default
                pnlStatus.Text = ""

                ' BPs were calcualted so enable it
                AddToShoppingListToolStripMenuItem.Enabled = True

            End If

        End If

        ' **********************************************************************
        ' *** Display results in grid - use for both calcuations and preview ***
        ' **********************************************************************
DisplayResults:

        ' Reset the columns before processing data
        Call RefreshManufacturingTabColumns()

        Dim NumManufacturingItems As Integer

        ' If no records first, then don't let them try and refresh nothing
        If IsNothing(FinalManufacturingItemList) And SavedRefreshValue Then
            Exit Sub
        End If

        If Not SavedRefreshValue Then
            ' Calc or new display data
            NumManufacturingItems = ManufacturingList.Count

            If NumManufacturingItems = 0 Then
                If Not Calculate Then
                    FinalManufacturingItemList = BaseItems ' Save for later use, this was just display
                Else
                    FinalManufacturingItemList = Nothing ' It didn't calculate anything, so just clear the grid and exit
                    lstManufacturing.Items.Clear()
                    GoTo ExitCalc
                End If
            Else
                ' Use Current data lists and save
                FinalManufacturingItemList = ManufacturingList
            End If
        Else
            ' Use pre-calc'd or loaded list
            NumManufacturingItems = FinalManufacturingItemList.Count
        End If

        ' Remove only but the optimal decryptor items before final display, and set the final list
        FinalItemList = SetOptimalDecryptorList(FinalManufacturingItemList, OptimalDecryptorItems)

        pnlProgressBar.Minimum = 0
        pnlProgressBar.Maximum = FinalItemList.Count
        pnlProgressBar.Value = 0
        pnlProgressBar.Visible = True

        lstManufacturing.Items.Clear()
        lstManufacturing.BeginUpdate()
        ' Disable sorting because it will crawl after we update if there are too many records
        lstManufacturing.ListViewItemSorter = Nothing
        lstManufacturing.SmallImageList = CalcImageList
        ' Set the formats before drawing
        lstManufacturing.SetRowFormats(ListRowFormats)

        pnlStatus.Text = "Refreshing List..."

        Dim BonusString As String = ""

        ' Load the final grid
        For i = 0 To FinalItemList.Count - 1
            Application.DoEvents()

            BPList = New ListViewItem(CStr(FinalItemList(i).ListID)) ' Always the first item

            If FinalItemList(i).DivideUnits = 0 Then
                ' So the display will show zeros instead of NaN (divide by zero)
                FinalItemList(i).DivideUnits = 1
            End If

            For j = 1 To ColumnPositions.Count - 1
                Select Case ColumnPositions(j)
                    Case ProgramSettings.ItemCategoryColumnName
                        BPList.SubItems.Add(FinalItemList(i).ItemCategory)
                    Case ProgramSettings.ItemGroupColumnName
                        BPList.SubItems.Add(FinalItemList(i).ItemGroup)
                    Case ProgramSettings.ItemNameColumnName
                        BPList.SubItems.Add(FinalItemList(i).ItemName)
                    Case ProgramSettings.OwnedColumnName
                        BPList.SubItems.Add(FinalItemList(i).Owned)
                    Case ProgramSettings.TechColumnName
                        BPList.SubItems.Add(FinalItemList(i).TechLevel)
                    Case ProgramSettings.BPMEColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).BPME))
                    Case ProgramSettings.BPTEColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).BPTE))
                    Case ProgramSettings.InputsColumnName
                        BPList.SubItems.Add(FinalItemList(i).Inputs)
                    Case ProgramSettings.ComparedColumnName
                        BPList.SubItems.Add(FinalItemList(i).CalcType)
                    Case ProgramSettings.TotalRunsColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).Runs))
                    Case ProgramSettings.SingleInventedBPCRunsColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).SingleInventedBPCRunsperBPC))
                    Case ProgramSettings.ProductionLinesColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ProductionLines))
                    Case ProgramSettings.LaboratoryLinesColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).LaboratoryLines))
                    Case ProgramSettings.TotalInventionCostColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).InventionCost / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.TotalCopyCostColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CopyCost / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.TaxesColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).Taxes / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.BrokerFeesColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).BrokerFees / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.BPProductionTimeColumnName
                        BPList.SubItems.Add(FinalItemList(i).BPProductionTime)
                    Case ProgramSettings.TotalProductionTimeColumnName
                        BPList.SubItems.Add(FinalItemList(i).TotalProductionTime)
                    Case ProgramSettings.CopyTimeColumnName
                        BPList.SubItems.Add(FinalItemList(i).CopyTime)
                    Case ProgramSettings.InventionTimeColumnName
                        BPList.SubItems.Add(FinalItemList(i).InventionTime)
                    Case ProgramSettings.ItemMarketPriceColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ItemMarketPrice / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.ProfitColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).Profit / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.ProfitPercentageColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).ProfitPercent, 2))
                    Case ProgramSettings.IskperHourColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).IPH, 2))
                    Case ProgramSettings.SVRColumnName
                        BPList.SubItems.Add(FinalItemList(i).SVR)
                    Case ProgramSettings.SVRxIPHColumnName
                        BPList.SubItems.Add(FinalItemList(i).SVRxIPH)
                    Case ProgramSettings.PriceTrendColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).PriceTrend, 2))
                    Case ProgramSettings.TotalItemsSoldColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).TotalItemsSold, 0))
                    Case ProgramSettings.TotalOrdersFilledColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).TotalOrdersFilled, 0))
                    Case ProgramSettings.AvgItemsperOrderColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).AvgItemsperOrder, 2))
                    Case ProgramSettings.CurrentSellOrdersColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CurrentSellOrders, 0))
                    Case ProgramSettings.CurrentBuyOrdersColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CurrentBuyOrders, 0))
                    Case ProgramSettings.ItemsinStockColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ItemsinStock, 0))
                    Case ProgramSettings.ItemsinProductionColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ItemsinProduction, 0))
                    Case ProgramSettings.TotalCostColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).TotalCost / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.BaseJobCostColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).BaseJobCost / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.NumBPsColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).NumBPs))
                    Case ProgramSettings.InventionChanceColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).InventionChance, 2))
                    Case ProgramSettings.BPTypeColumnName
                        BPList.SubItems.Add(GetBPTypeString(FinalItemList(i).BlueprintType))
                    Case ProgramSettings.RaceColumnName
                        BPList.SubItems.Add(FinalItemList(i).Race)
                    Case ProgramSettings.VolumeperItemColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).VolumeperItem, 2))
                    Case ProgramSettings.TotalVolumeColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).TotalVolume / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.PortionSizeColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).PortionSize, 0))

                    Case ProgramSettings.ManufacturingJobFeeColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).JobFee / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.ManufacturingFacilityNameColumnName
                        BPList.SubItems.Add(FinalItemList(i).ManufacturingFacility.FacilityName)
                    Case ProgramSettings.ManufacturingFacilitySystemColumnName
                        BPList.SubItems.Add(FinalItemList(i).ManufacturingFacility.SolarSystemName)
                    Case ProgramSettings.ManufacturingFacilitySystemIndexColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ManufacturingFacility.CostIndex, 5))
                    Case ProgramSettings.ManufacturingFacilityTaxColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).ManufacturingFacility.TaxRate, 1))
                    Case ProgramSettings.ManufacturingFacilityRegionColumnName
                        BPList.SubItems.Add(FinalItemList(i).ManufacturingFacility.RegionName)
                    Case ProgramSettings.ManufacturingFacilityMEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ManufacturingFacility.MaterialMultiplier))
                    Case ProgramSettings.ManufacturingFacilityTEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ManufacturingFacility.TimeMultiplier))
                    Case ProgramSettings.ManufacturingFacilityUsageColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ManufacturingFacilityUsage / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.ManufacturingFacilityFWSystemLevelColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ManufacturingFacility.FWUpgradeLevel))

                    Case ProgramSettings.ComponentFacilityNameColumnName
                        BPList.SubItems.Add(FinalItemList(i).ComponentManufacturingFacility.FacilityName)
                    Case ProgramSettings.ComponentFacilitySystemColumnName
                        BPList.SubItems.Add(FinalItemList(i).ComponentManufacturingFacility.SolarSystemName)
                    Case ProgramSettings.ComponentFacilitySystemIndexColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ComponentManufacturingFacility.CostIndex, 5))
                    Case ProgramSettings.ComponentFacilityTaxColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).ComponentManufacturingFacility.TaxRate, 1))
                    Case ProgramSettings.ComponentFacilityRegionColumnName
                        BPList.SubItems.Add(FinalItemList(i).ComponentManufacturingFacility.RegionName)
                    Case ProgramSettings.ComponentFacilityMEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ComponentManufacturingFacility.MaterialMultiplier))
                    Case ProgramSettings.ComponentFacilityTEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ComponentManufacturingFacility.TimeMultiplier))
                    Case ProgramSettings.ComponentFacilityUsageColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).ComponentManufacturingFacilityUsage / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.ComponentFacilityFWSystemLevelColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).ComponentManufacturingFacility.FWUpgradeLevel))

                    Case ProgramSettings.CapComponentFacilityNameColumnName
                        BPList.SubItems.Add(FinalItemList(i).CapComponentManufacturingFacility.FacilityName)
                    Case ProgramSettings.CapComponentFacilitySystemColumnName
                        BPList.SubItems.Add(FinalItemList(i).CapComponentManufacturingFacility.SolarSystemName)
                    Case ProgramSettings.CapComponentFacilitySystemIndexColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CapComponentManufacturingFacility.CostIndex, 5))
                    Case ProgramSettings.CapComponentFacilityTaxColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).CapComponentManufacturingFacility.TaxRate, 1))
                    Case ProgramSettings.CapComponentFacilityRegionColumnName
                        BPList.SubItems.Add(FinalItemList(i).CapComponentManufacturingFacility.RegionName)
                    Case ProgramSettings.CapComponentFacilityMEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).CapComponentManufacturingFacility.MaterialMultiplier))
                    Case ProgramSettings.CapComponentFacilityTEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).CapComponentManufacturingFacility.TimeMultiplier))
                    Case ProgramSettings.CapComponentFacilityUsageColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CapComponentManufacturingFacilityUsage / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.CapComponentFacilityFWSystemLevelColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).CapComponentManufacturingFacility.FWUpgradeLevel))

                    Case ProgramSettings.CopyingFacilityNameColumnName
                        BPList.SubItems.Add(FinalItemList(i).CopyFacility.FacilityName)
                    Case ProgramSettings.CopyingFacilitySystemColumnName
                        BPList.SubItems.Add(FinalItemList(i).CopyFacility.SolarSystemName)
                    Case ProgramSettings.CopyingFacilitySystemIndexColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CopyFacility.CostIndex, 5))
                    Case ProgramSettings.CopyingFacilityTaxColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).CopyFacility.TaxRate, 1))
                    Case ProgramSettings.CopyingFacilityRegionColumnName
                        BPList.SubItems.Add(FinalItemList(i).CopyFacility.RegionName)
                    Case ProgramSettings.CopyingFacilityMEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).CopyFacility.MaterialMultiplier))
                    Case ProgramSettings.CopyingFacilityTEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).CopyFacility.TimeMultiplier))
                    Case ProgramSettings.CopyingFacilityUsageColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).CopyFacilityUsage / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.CopyingFacilityFWSystemLevelColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).CopyFacility.FWUpgradeLevel))

                    Case ProgramSettings.InventionFacilityNameColumnName
                        BPList.SubItems.Add(FinalItemList(i).InventionFacility.FacilityName)
                    Case ProgramSettings.InventionFacilitySystemColumnName
                        BPList.SubItems.Add(FinalItemList(i).InventionFacility.SolarSystemName)
                    Case ProgramSettings.InventionFacilitySystemIndexColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).InventionFacility.CostIndex, 5))
                    Case ProgramSettings.InventionFacilityTaxColumnName
                        BPList.SubItems.Add(FormatPercent(FinalItemList(i).InventionFacility.TaxRate, 1))
                    Case ProgramSettings.InventionFacilityRegionColumnName
                        BPList.SubItems.Add(FinalItemList(i).InventionFacility.RegionName)
                    Case ProgramSettings.InventionFacilityMEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).InventionFacility.MaterialMultiplier))
                    Case ProgramSettings.InventionFacilityTEBonusColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).InventionFacility.TimeMultiplier))
                    Case ProgramSettings.InventionFacilityUsageColumnName
                        BPList.SubItems.Add(FormatNumber(FinalItemList(i).InventionFacilityUsage / FinalItemList(i).DivideUnits, 2))
                    Case ProgramSettings.InventionFacilityFWSystemLevelColumnName
                        BPList.SubItems.Add(CStr(FinalItemList(i).InventionFacility.FWUpgradeLevel))
                End Select
            Next

            ' Add the record
            Call lstManufacturing.Items.Add(BPList)

            ' For each record, update the progress bar
            Call IncrementToolStripProgressBar(pnlProgressBar)

        Next

        Dim TempType As SortOrder

        ' Now sort this
        If ManufacturingColumnSortType = SortOrder.Ascending Then
            TempType = SortOrder.Descending
        Else
            TempType = SortOrder.Ascending
        End If

        ' Sort the list based on the saved column, if they change the number of columns below value, then find IPH, if not there, use column 0
        If ManufacturingColumnClicked > lstManufacturing.Columns.Count Then
            ' Find the IPH column
            If UserManufacturingTabColumnSettings.IskperHour <> 0 Then
                ManufacturingColumnClicked = UserManufacturingTabColumnSettings.IskperHour
            Else
                ManufacturingColumnClicked = 0 ' Default, will always be there
            End If

        End If

        ' Sort away
        Call ListViewColumnSorter(ManufacturingColumnClicked, CType(lstManufacturing, ListView), ManufacturingColumnClicked, TempType)

        lstManufacturing.EndUpdate()

ExitCalc:
        pnlProgressBar.Value = 0
        pnlProgressBar.Visible = False
        pnlStatus.Text = ""
        lstManufacturing.EndUpdate()

        ' Enable all the controls
        btnCalcPreview.Enabled = True
        btnCalcReset.Enabled = True
        btnCalcSelectColumns.Enabled = True
        btnCalcSaveSettings.Enabled = True
        btnCalcExportList.Enabled = True
        gbCalcMarketFilters.Enabled = True
        gbCalcBPSelect.Enabled = True
        gbCalcBPTech.Enabled = True
        gbCalcCompareType.Enabled = True
        gbCalcFilter.Enabled = True
        gbCalcIgnoreinCalcs.Enabled = True
        gbCalcIncludeOwned.Enabled = True
        gbCalcInvention.Enabled = True
        gbCalcProdLines.Enabled = True
        gbCalcRelics.Enabled = True
        gbCalcTextColors.Enabled = True
        gbCalcTextFilter.Enabled = True
        lstManufacturing.Enabled = True
        tabCalcFacilities.Enabled = True

        Application.UseWaitCursor = False
        Me.Cursor = Cursors.Default
        Application.DoEvents()

        If lstManufacturing.Items.Count = 0 And Not CancelManufacturingTabCalc Then
            MsgBox("No Blueprints calculated for options selected.", vbExclamation, Application.ProductName)
        End If

        If Not Calculate Or CancelManufacturingTabCalc Then
            Call ResetRefresh()
            CancelManufacturingTabCalc = False
        Else
            btnCalcCalculate.Text = "Refresh"
            RefreshCalcData = True ' Allow data to be refreshed since we just calcuated
        End If

    End Sub

    ' Finds the total items sold over the time period for the region sent
    Private Function CalculateTotalItemsSold(ByVal TypeID As Long, ByVal RegionID As Long, DaysfromToday As Integer) As Long
        Dim SQL As String
        Dim rsItems As SQLiteDataReader

        SQL = "SELECT SUM(TOTAL_VOLUME_FILLED) FROM MARKET_HISTORY WHERE TYPE_ID = " & CStr(TypeID) & " AND REGION_ID = " & CStr(RegionID) & " "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) >= " & " DateTime('" & Format(DateAdd(DateInterval.Day, -(DaysfromToday + 1), Now.Date), SQLiteDateFormat) & "') "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) < " & " DateTime('" & Format(Now.Date, SQLiteDateFormat) & "') "
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsItems = DBCommand.ExecuteReader

        If rsItems.Read() And Not IsDBNull(rsItems.GetValue(0)) Then
            Return rsItems.GetInt64(0)
        Else
            Return 0
        End If

    End Function

    ' Finds the total orders filled over the time period for the region sent
    Private Function CalculateTotalOrdersFilled(ByVal TypeID As Long, ByVal RegionID As Long, DaysfromToday As Integer) As Long
        Dim SQL As String
        Dim rsItems As SQLiteDataReader

        SQL = "SELECT SUM(TOTAL_ORDERS_FILLED) FROM MARKET_HISTORY WHERE TYPE_ID = " & CStr(TypeID) & " AND REGION_ID = " & CStr(RegionID) & " "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) >= " & " DateTime('" & Format(DateAdd(DateInterval.Day, -(DaysfromToday + 1), Now.Date), SQLiteDateFormat) & "') "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) < " & " DateTime('" & Format(Now.Date, SQLiteDateFormat) & "') "
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsItems = DBCommand.ExecuteReader

        If rsItems.Read() And Not IsDBNull(rsItems.GetValue(0)) Then
            Return rsItems.GetInt64(0)
        Else
            Return 0
        End If

    End Function

    ' Finds the average items sold per order over the time period for the region sent, and sets the two by reference
    Private Sub GetCurrentOrders(ByVal TypeID As Long, ByVal RegionID As Long, ByRef BuyOrders As Long, ByRef SellOrders As Long)
        Dim SQL As String
        Dim rsItems As SQLiteDataReader

        SQL = "SELECT IS_BUY_ORDER, SUM(VOLUME_REMAINING) FROM MARKET_ORDERS WHERE TYPE_ID = " & CStr(TypeID) & " AND REGION_ID = " & CStr(RegionID) & " "
        SQL = SQL & "GROUP BY IS_BUY_ORDER"
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsItems = DBCommand.ExecuteReader

        While rsItems.Read
            If rsItems.GetInt32(0) = 0 Then
                SellOrders = rsItems.GetInt64(1)
            Else
                BuyOrders = rsItems.GetInt64(1)
            End If
        End While

    End Sub

    ' Finds the number of items in stock for the asset settings set here
    Private Function GetTotalItemsinStock(ByVal TypeID As Long) As Integer
        Dim SQL As String
        Dim readerAssets As SQLiteDataReader
        Dim CurrentItemName As String = ""
        Dim ItemQuantity As Integer = 0

        Application.UseWaitCursor = True
        Me.Cursor = Cursors.WaitCursor
        Application.DoEvents()

        Dim IDString As String = ""

        ' Set the ID string we will use to update
        If UserAssetWindowShoppingListSettings.AssetType = "Both" Then
            IDString = CStr(SelectedCharacter.ID) & "," & CStr(SelectedCharacter.CharacterCorporation.CorporationID)
        ElseIf UserAssetWindowShoppingListSettings.AssetType = "Personal" Then
            IDString = CStr(SelectedCharacter.ID)
        ElseIf UserAssetWindowShoppingListSettings.AssetType = "Corporation" Then
            IDString = CStr(SelectedCharacter.CharacterCorporation.CorporationID)
        End If

        ' Build the where clause to look up data
        Dim AssetLocationFlagList As New List(Of String)
        ' First look up the location and flagID pairs - unique ID of asset locations
        SQL = "SELECT LocationID, FlagID FROM ASSET_LOCATIONS WHERE EnumAssetType = " & CStr(AssetWindow.ManufacturingTab) & " AND ID IN (" & IDString & ")"
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerAssets = DBCommand.ExecuteReader

        While readerAssets.Read
            If readerAssets.GetInt32(1) = -4 Then
                ' If the flag is the base location, then we want all items at the location id
                AssetLocationFlagList.Add("(LocationID = " & CStr(readerAssets.GetInt64(0)) & ")")
            Else
                AssetLocationFlagList.Add("(LocationID = " & CStr(readerAssets.GetInt64(0)) & " AND Flag = " & CStr(readerAssets.GetInt32(1)) & ")")
            End If
        End While

        readerAssets.Close()

        ' Look up each item in their assets in their locations stored, and sum up the quantity'
        ' Split into groups to run (1000 identifiers max so limit to 900)
        Dim Splits As Integer = CInt(Math.Ceiling(AssetLocationFlagList.Count / 900))
        For k = 0 To Splits - 1
            Application.DoEvents()
            Dim TempAssetWhereList As String = ""
            ' Build the partial asset location id/flag list
            For z = k * 900 To (k + 1) * 900 - 1
                If z = AssetLocationFlagList.Count Then
                    ' exit if we get to the end of the list
                    Exit For
                End If
                TempAssetWhereList = TempAssetWhereList & AssetLocationFlagList(z) & " OR "
            Next

            ' Strip final OR
            TempAssetWhereList = TempAssetWhereList.Substring(0, Len(TempAssetWhereList) - 4)

            SQL = "SELECT SUM(Quantity) FROM ASSETS WHERE (" & TempAssetWhereList & ") "
            SQL = SQL & " AND ASSETS.TypeID = " & CStr(TypeID) & " AND ID IN (" & IDString & ")"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerAssets = DBCommand.ExecuteReader
            readerAssets.Read()

            If readerAssets.HasRows And Not IsDBNull(readerAssets.GetValue(0)) Then
                ItemQuantity += readerAssets.GetInt32(0) ' sum up
            End If

        Next

        Return ItemQuantity

    End Function

    ' Finds the number of items in production from all loaded characters
    Private Function GetTotalItemsinProduction(ByVal TypeID As Long) As Integer
        Dim SQL As String
        Dim rsItems As SQLiteDataReader

        SQL = "SELECT SUM(runs * PORTION_SIZE) FROM INDUSTRY_JOBS, ALL_BLUEPRINTS WHERE INDUSTRY_JOBS.productTypeID = ALL_BLUEPRINTS.ITEM_ID "
        SQL = SQL & "AND productTypeID = " & CStr(TypeID) & " AND status = 1 AND activityID = 1 "
        'SQL = SQL & "AND INSTALLER_ID = " & CStr(SelectedCharacter.ID) & " "
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsItems = DBCommand.ExecuteReader

        If rsItems.Read() And Not IsDBNull(rsItems.GetValue(0)) Then
            Return rsItems.GetInt32(0)
        Else
            Return 0
        End If

    End Function

    ' Sets the name of the array to use on the pos for multiuse arrays
    Private Function GetCalcPOSMultiUseArrayName(ShortName As String) As String

        Select Case ShortName
            Case "Equipment"
                Return "Equipment Assembly Array"
            Case "Rapid"
                Return "Rapid Equipment Assembly Array"
            Case "Ammunition"
                Return "Ammunition Assembly Array"
            Case "Component"
                Return "Component Assembly Array"
            Case "Large"
                Return "Large Ship Assembly Array"
            Case "Capital"
                Return "Capital Ship Assembly Array"
            Case "All"
                Return "All"
        End Select

        Return ""

    End Function

    ' Sets the name of the array to the short name when long name sent
    Private Function GetTruncatedCalcPOSMultiUseArrayName(LongName As String) As String

        Select Case LongName
            Case "Equipment Assembly Array"
                Return "Equipment"
            Case "Rapid Equipment Assembly Array"
                Return "Rapid"
            Case "Ammunition Assembly Array"
                Return "Ammunition"
            Case "Component Assembly Array"
                Return "Component"
            Case "Large Ship Assembly Array"
                Return "Large"
            Case "Capital Ship Assembly Array"
                Return "Capital"
            Case "All"
                Return "All"
        End Select

        Return ""

    End Function

    ' Sets the owned flag for an insert item
    Private Function SetItemOwnedFlag(ByRef SentItem As ManufacturingItem, ByVal SentOrigDecryptor As Decryptor, ByVal SentOrigRelic As String,
                                 ByVal SentOrigME As Integer, ByVal SentOrigTE As Integer, ByVal SentOriginalBPOwnedFlag As Boolean) As Boolean
        ' We know the original decryptor and relic used for this bp so see if they match what we just 
        ' used and set the owned flag and it's invented, which all these are - also make sure the me/te are same
        ' as base if no decryptor used
        If SentItem.Decryptor.Name = SentOrigDecryptor.Name And SentOrigRelic.Contains(SentItem.Relic) _
            And SentOriginalBPOwnedFlag = True And SentItem.BlueprintType = BPType.InventedBPC _
            And Not (SentOrigDecryptor.Name = NoDecryptor.Name And SentOrigME <> BaseT2T3ME And SentOrigTE <> BaseT2T3TE) Then
            SentItem.Owned = Yes
            Return True
        Else
            SentItem.Owned = No
            Return False
        End If
    End Function

    ' Loads the cmbBPTypeFilter object with types based on the radio button selected - Ie, Drones will load Drone types (Small, Medium, Heavy...etc)
    Private Sub LoadCalcBPTypes()
        Dim SQL As String
        Dim WhereClause As String = ""
        Dim readerTypes As SQLiteDataReader
        Dim InventedBPs As New List(Of Long)

        cmbCalcBPTypeFilter.Text = UserManufacturingTabSettings.ItemTypeFilter
        SQL = "SELECT ITEM_GROUP FROM " & USER_BLUEPRINTS

        WhereClause = BuildManufactureWhereClause(True, InventedBPs)

        If WhereClause = "" Then
            ' They didn't select anything, just clear and exit
            cmbCalcBPTypeFilter.Items.Clear()
            cmbCalcBPTypeFilter.Text = "All Types"
            Exit Sub
        End If

        ' See if we are looking at User Owned blueprints or All
        If rbtnCalcBPOwned.Checked Then
            WhereClause = WhereClause & "AND USER_ID = " & SelectedCharacter.ID & " AND OWNED <> 0  "
        End If

        SQL = SQL & WhereClause & "GROUP BY ITEM_GROUP"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        DBCommand.Parameters.AddWithValue("@USERBP_USERID", CStr(SelectedCharacter.ID)) ' need to search for corp ID too
        DBCommand.Parameters.AddWithValue("@USERBP_CORPID", CStr(SelectedCharacter.CharacterCorporation.CorporationID))
        readerTypes = DBCommand.ExecuteReader

        cmbCalcBPTypeFilter.Items.Clear()

        cmbCalcBPTypeFilter.Items.Add("All Types")

        While readerTypes.Read
            cmbCalcBPTypeFilter.Items.Add(readerTypes.GetString(0))
        End While

    End Sub

    ' Just adds an item into the list and duplicates if raw or components checked
    Private Sub InsertItemCalcType(ByRef ManufacturingItemList As List(Of ManufacturingItem), ByVal BaseItem As ManufacturingItem,
                                   ByVal AddMultipleFacilities As Boolean, ByVal FacilityList As List(Of IndustryFacility), ByRef FormatList As List(Of RowFormat))

        Dim CalcType As String = ""
        Dim TempItem As New ManufacturingItem
        Dim CurrentRowFormat As New RowFormat

        If rbtnCalcCompareRawMats.Checked Then
            CalcType = "Raw Mats"
        ElseIf rbtnCalcCompareComponents.Checked Then
            CalcType = "Components"
        ElseIf rbtnCalcCompareBuildBuy.Checked Then
            CalcType = "Build/Buy"
        Else ' All
            CalcType = "All Calcs"
        End If

        If AddMultipleFacilities Then
            For i = 0 To FacilityList.Count - 1
                ' Set data
                TempItem = CType(BaseItem.Clone, ManufacturingItem)
                ListIDIterator += 1
                TempItem.ListID = ListIDIterator
                TempItem.ManufacturingFacility = CType(FacilityList(i).Clone(), IndustryFacility)
                TempItem.CalcType = CalcType
                ' Add it
                ManufacturingItemList.Add(TempItem)
                ' Reset the Item
                TempItem = New ManufacturingItem
            Next
        Else
            TempItem = CType(BaseItem.Clone, ManufacturingItem)
            ListIDIterator += 1
            TempItem.ListID = ListIDIterator
            TempItem.CalcType = CalcType

            ManufacturingItemList.Add(TempItem)
        End If

        ' Set the list row format for just display, after calcs it will reset
        ' Now determine the format of the item and save it for drawing the list
        CurrentRowFormat.ListID = TempItem.ListID

        'Set the row format for background and foreground colors
        'All columns need to be colored properly
        ' Color owned BP's
        If TempItem.Owned = Yes Then
            If TempItem.Scanned = 1 Or TempItem.Scanned = 0 Then
                CurrentRowFormat.BackColor = Brushes.BlanchedAlmond
            ElseIf TempItem.Scanned = 2 Then
                ' Corp owned
                CurrentRowFormat.BackColor = Brushes.LightGreen
            End If
        ElseIf UserInventedBPs.Contains(TempItem.BPID) Then
            ' It's an invented BP that we own the T1 BP for
            CurrentRowFormat.BackColor = Brushes.LightSteelBlue
        Else
            CurrentRowFormat.BackColor = Brushes.White
        End If

        ' Set default and change if needed
        CurrentRowFormat.ForeColor = Brushes.Black

        ' Insert the format
        FormatList.Add(CurrentRowFormat)

    End Sub

    ' Exports the list to clipboard
    Private Sub btnCalcExportList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCalcExportList.Click
        Dim MyStream As StreamWriter
        Dim FileName As String
        Dim OutputText As String
        Dim Price As ListViewItem
        Dim Separator As String = ""
        Dim Items As ListView.ListViewItemCollection
        Dim ExportColumns As New List(Of String)
        Dim NumItems As Integer = 0

        If UserApplicationSettings.DataExportFormat = SSVDataExport Then
            ' Save file name with date
            FileName = "Manufacturing Calculations Export - " & Format(Now, "MMddyyyy") & ".ssv"

            ' Show the dialog
            SaveFileDialog.Filter = "ssv files (*.ssv)|*.ssv|All files (*.*)|*.*"
            Separator = ";"
        Else ' All others in CSV for now
            ' Save file name with date
            FileName = "Manufacturing Calculations Export - " & Format(Now, "MMddyyyy") & ".csv"

            ' Show the dialog
            SaveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            Separator = ","
        End If

        SaveFileDialog.FilterIndex = 1
        SaveFileDialog.RestoreDirectory = True
        SaveFileDialog.FileName = FileName

        If SaveFileDialog.ShowDialog() = DialogResult.OK Then
            Try
                MyStream = File.CreateText(SaveFileDialog.FileName)

                If Not (MyStream Is Nothing) Then

                    Items = lstManufacturing.Items

                    If Items.Count > 0 Then
                        Me.Cursor = Cursors.WaitCursor
                        pnlProgressBar.Minimum = 0
                        pnlProgressBar.Maximum = Items.Count - 1
                        pnlProgressBar.Value = 0
                        pnlProgressBar.Visible = True
                        pnlStatus.Text = "Exporting Table..."
                        Application.DoEvents()

                        OutputText = ""
                        For i = 1 To ColumnPositions.Count - 1
                            If ColumnPositions(i) <> "" Then
                                OutputText = OutputText & ColumnPositions(i) & Separator
                                ExportColumns.Add(ColumnPositions(i))
                            End If
                        Next
                        OutputText = OutputText.Substring(0, Len(OutputText) - 1) ' Strip last separator

                        MyStream.Write(OutputText & Environment.NewLine)

                        For Each Price In Items
                            OutputText = ""
                            For j = 0 To ExportColumns.Count - 1
                                ' Format each column value and save
                                OutputText = OutputText & GetOutputText(ExportColumns(j), Price.SubItems(j + 1).Text, Separator, UserApplicationSettings.DataExportFormat)
                            Next

                            ' For each record, update the progress bar
                            Call IncrementToolStripProgressBar(pnlProgressBar)
                            Application.DoEvents()

                            MyStream.Write(OutputText & Environment.NewLine)
                        Next

                        MyStream.Flush()
                        MyStream.Close()

                        MsgBox("Manufacturing Data Exported", vbInformation, Application.ProductName)

                    End If
                End If
            Catch
                MsgBox(Err.Description, vbExclamation, Application.ProductName)
            End Try
        End If

        ' Done processing the blueprints
        pnlProgressBar.Value = 0
        pnlProgressBar.Visible = False

        gbCalcBPSelectOptions.Enabled = True
        Me.Cursor = Cursors.Default
        Me.Refresh()
        Application.DoEvents()
        pnlStatus.Text = ""

    End Sub

    ' Outputs text in the correct format
    Private Function GetOutputText(ColumnName As String, DataText As String, Separator As String, ExportDataType As String) As String
        Dim ExportData As String

        Select Case ColumnName
            Case ProgramSettings.InventionChanceColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.VolumeperItemColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TotalVolumeColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TotalInventionCostColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TotalCopyCostColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TaxesColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.BrokerFeesColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ItemMarketPriceColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ProfitColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.IskperHourColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.SVRColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.SVRxIPHColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TotalItemsSoldColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TotalOrdersFilledColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.AvgItemsperOrderColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.CurrentSellOrdersColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.CurrentBuyOrdersColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ItemsinProductionColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ItemsinStockColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.TotalCostColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.BaseJobCostColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ManufacturingJobFeeColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ManufacturingFacilitySystemIndexColumnName
                ExportData = FormatNumber(DataText, 5) & Separator
            Case ProgramSettings.ManufacturingFacilityUsageColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.ComponentFacilitySystemIndexColumnName
                ExportData = FormatNumber(DataText, 5) & Separator
            Case ProgramSettings.ComponentFacilityUsageColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.CapComponentFacilitySystemIndexColumnName
                ExportData = FormatNumber(DataText, 5) & Separator
            Case ProgramSettings.CapComponentFacilityUsageColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.CopyingFacilitySystemIndexColumnName
                ExportData = FormatNumber(DataText, 5) & Separator
            Case ProgramSettings.CopyingFacilityUsageColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.InventionFacilitySystemIndexColumnName
                ExportData = FormatNumber(DataText, 5) & Separator
            Case ProgramSettings.InventionFacilityUsageColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case ProgramSettings.PortionSizeColumnName
                ExportData = Format(DataText, "Fixed") & Separator
            Case Else
                ExportData = DataText & Separator
        End Select

        If ExportDataType = SSVDataExport Then
            ' Format to EU
            ExportData = ConvertUStoEUDecimal(ExportData)
        End If

        Return ExportData

    End Function

    ' Refresh the list with blueprints before we calculate the data so the user knows what they are calculating
    Private Sub btnManufactureRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCalcPreview.Click
        Call DisplayManufacturingResults(False)
    End Sub

    ' Reads through the manufacturing blueprint list and calculates the isk per hour for all that are selected, then sorts them and displays
    Private Sub btnCalculate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCalcCalculate.Click
        If btnCalcCalculate.Text = "Cancel" Then
            CancelManufacturingTabCalc = True
        Else
            Call DisplayManufacturingResults(True)
        End If
    End Sub

    ' Builds the query for the main grid update
    Private Function BuildManufacturingSelectQuery(ByRef RecordCount As Integer, ByRef InventedBPs As List(Of Long)) As String
        Dim SQL As String = ""
        Dim SQLTemp As String = ""
        Dim WhereClause As String = ""
        Dim ComboType As String = ""

        ' Core Query
        SQL = "SELECT * FROM " & USER_BLUEPRINTS

        WhereClause = BuildManufactureWhereClause(False, InventedBPs)

        ' Don't load if no where clause
        If WhereClause = "" Then
            Return ""
        End If

        ' Get the record count first
        SQLTemp = "SELECT COUNT(*) FROM " & USER_BLUEPRINTS & WhereClause

        Dim CMDCount As New SQLiteCommand(SQLTemp, EVEDB.DBREf)
        CMDCount.Parameters.AddWithValue("@USERBP_USERID", CStr(SelectedCharacter.ID)) ' need to search for corp ID too
        CMDCount.Parameters.AddWithValue("@USERBP_CORPID", CStr(SelectedCharacter.CharacterCorporation.CorporationID))
        RecordCount = CInt(CMDCount.ExecuteScalar())

        Return SQL & WhereClause & " ORDER BY ITEM_GROUP, ITEM_NAME"

    End Function

    ' Builds the where clause for the calc screen based on Tech and Group selections, by reference will return the list of Invented BPs
    Private Function BuildManufactureWhereClause(LoadingList As Boolean, ByRef InventedBPs As List(Of Long)) As String
        Dim WhereClause As String = ""
        Dim ItemTypes As String = ""
        Dim ComboType As String = ""
        Dim ItemTypeNumbers As String = ""
        Dim T2Selected As Boolean = False ' Whether the user wants to look at T2 blueprints or not - this is used in loading only T2 bps that we can invent
        Dim readerT1s As SQLiteDataReader
        Dim TempRace As String = ""
        Dim RaceClause As String = ""
        Dim SizesClause As String = ""

        Dim SQL As String = ""
        Dim T2Query As String = ""
        Dim T3Query As String = ""
        Dim RelicRuns As String = ""

        ' Items
        If chkCalcAmmo.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY = 'Charge' OR "
        End If
        If chkCalcDrones.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY IN ('Drone', 'Fighter') OR "
        End If
        If chkCalcModules.Checked Then
            ItemTypes = ItemTypes & "(X.ITEM_CATEGORY = 'Module' AND X.ITEM_GROUP NOT LIKE 'Rig%') OR "
        End If
        If chkCalcShips.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY = 'Ship' OR "
        End If
        If chkCalcSubsystems.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY = 'Subsystem' OR "
        End If
        If chkCalcBoosters.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY = 'Implant' OR "
        End If
        If chkCalcComponents.Checked Then
            ItemTypes = ItemTypes & "(X.ITEM_GROUP LIKE '%Components%' AND X.ITEM_GROUP <> 'Station Components') OR "
        End If
        If chkCalcRigs.Checked Then
            ItemTypes = ItemTypes & "(X.BLUEPRINT_GROUP = 'Rig Blueprint' OR (X.ITEM_CATEGORY = 'Structure Module' AND X.ITEM_GROUP LIKE '%Rig%')) OR "
        End If
        If chkCalcStructureRigs.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY = 'Structure Rigs' OR "
        End If
        If chkCalcCelestials.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY IN ('Celestial', 'Orbitals', 'Sovereignty Structures', 'Station', 'Accessories') OR "
        End If
        If chkCalcStructureModules.Checked Then
            ItemTypes = ItemTypes & "(X.ITEM_CATEGORY = 'Structure Module' AND X.ITEM_GROUP NOT LIKE '%Rig%') OR "
        End If
        If chkCalcReactions.Checked Then
            ItemTypes = ItemTypes & "(X.BLUEPRINT_GROUP LIKE '%Reaction Formulas') OR "
        End If
        If chkCalcMisc.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_GROUP IN ('Tool','Data Interfaces','Cyberimplant','Fuel Block') OR "
        End If
        If chkCalcDeployables.Checked Then
            ItemTypes = ItemTypes & "X.ITEM_CATEGORY = 'Deployable' OR "
        End If
        If chkCalcStructures.Checked Then
            ItemTypes = ItemTypes & "(X.ITEM_CATEGORY IN ('Starbase','Structure') OR X.ITEM_GROUP = 'Station Components')  OR "
        End If

        ' Take off last OR
        If ItemTypes <> "" Then
            ItemTypes = ItemTypes.Substring(0, ItemTypes.Count - 4)
        Else
            ' Can't run this
            Return ""
        End If

        ' Item Type Definitions - These are set by me based on existing data
        ' 1, 2, 14 are T1, T2, T3
        ' 3 is Storyline
        ' 15 is Pirate Faction
        ' 16 is Navy Faction

        ' Check Tech version
        If chkCalcT1.Enabled Then
            ' Only a Subsystem so T3
            If chkCalcT1.Checked Then
                ItemTypeNumbers = ItemTypeNumbers & "1,"
            End If
        End If

        If chkCalcT2.Enabled Then
            If chkCalcT2.Checked Then
                ' If we have T2 blueprints and they selected to only have T2 they have T1 blueprints for to invent
                ' then build this list and add a special SQL item type entry for T2's
                If rbtnCalcAllBPs.Checked Or chkCalcIncludeT2Owned.Checked Then
                    InventedBPs = New List(Of Long)
                    ' Select all the T2 bps that we can invent from our owned bps and save them
                    SQL = "SELECT productTypeID FROM INDUSTRY_ACTIVITY_PRODUCTS "
                    SQL = SQL & "WHERE activityID = 8 AND blueprintTypeID IN "
                    SQL = SQL & "(SELECT BP_ID FROM " & USER_BLUEPRINTS & " WHERE "
                    If rbtnCalcBPFavorites.Checked Then
                        SQL = SQL & " X.FAVORITE = 1 AND "
                    Else
                        SQL = SQL & " X.OWNED <> 0 AND "
                    End If
                    SQL = SQL & "X.ITEM_TYPE = 1) GROUP BY productTypeID"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    DBCommand.Parameters.AddWithValue("@USERBP_USERID", CStr(SelectedCharacter.ID)) ' need to search for corp ID too
                    DBCommand.Parameters.AddWithValue("@USERBP_CORPID", CStr(SelectedCharacter.CharacterCorporation.CorporationID))
                    readerT1s = DBCommand.ExecuteReader()

                    While readerT1s.Read
                        ' Build list for where clause
                        T2Query = T2Query & CStr(readerT1s.GetValue(0)) & ","
                        ' Save the T2 BPID for later lookup to display
                        InventedBPs.Add(CLng(readerT1s.GetValue(0)))
                    End While

                    readerT1s.Close()
                    readerT1s = Nothing
                    DBCommand = Nothing

                    ' Set the list of T2 BPC's we want and allow for User ID 0 (not owned but invented) or the User ID (OWNED)
                    If InventedBPs.Count <> 0 And T2Query <> "" Then
                        T2Query = " OR (X.ITEM_TYPE = 2 AND X.BP_ID IN (" & T2Query.Substring(0, T2Query.Length - 1) & ")) "
                    End If
                Else
                    T2Query = ""
                End If

                ItemTypeNumbers = ItemTypeNumbers & "2,"

            End If
        End If

        If chkCalcT3.Enabled Then
            If chkCalcT3.Checked Then
                If rbtnCalcAllBPs.Checked Or chkCalcIncludeT3Owned.Checked Then
                    T3Query = " OR (X.ITEM_TYPE = 14) "
                Else
                    T3Query = ""
                End If

                ItemTypeNumbers = ItemTypeNumbers & "14,"

            End If
        End If

        If chkCalcStoryline.Enabled Then
            If chkCalcStoryline.Checked Then
                ItemTypeNumbers = ItemTypeNumbers & "3,"
            End If
        End If

        If chkCalcPirateFaction.Enabled Then
            If chkCalcPirateFaction.Checked Then
                ItemTypeNumbers = ItemTypeNumbers & "15,"
            End If
        End If

        If chkCalcNavyFaction.Enabled Then
            If chkCalcNavyFaction.Checked Then
                ItemTypeNumbers = ItemTypeNumbers & "16,"
            End If
        End If

        ' Add Item Type
        If ItemTypeNumbers <> "" Then
            ItemTypeNumbers = "X.ITEM_TYPE IN (" & ItemTypeNumbers.Substring(0, ItemTypeNumbers.Length - 1) & ") "
        Else
            ' They need to have at least one tech. If not, just return nothing
            Return ""
        End If

        ' See if we are looking at User Owned blueprints or item types and add this - only want owned item types
        If rbtnCalcBPOwned.Checked Then
            ItemTypeNumbers = ItemTypeNumbers & " AND OWNED <> 0 "
        End If

        ' Determine what race we are looking at
        If chkCalcRaceAmarr.Checked Then
            TempRace = TempRace & "4,"
        End If
        If chkCalcRaceCaldari.Checked Then
            TempRace = TempRace & "1,"
        End If
        If chkCalcRaceMinmatar.Checked Then
            TempRace = TempRace & "2,"
        End If
        If chkCalcRaceGallente.Checked Then
            TempRace = TempRace & "8,"
        End If
        If chkCalcRacePirate.Checked Then
            TempRace = TempRace & "15,"
        End If
        If chkCalcRaceOther.Checked Then
            TempRace = TempRace & "0,"
        End If

        If TempRace <> "" Then
            TempRace = "(" & TempRace.Substring(0, Len(TempRace) - 1) & ")"
            RaceClause = "X.RACE_ID IN " & TempRace
        Else
            ' They need to have at least one. If not, just return nothing
            Return ""
        End If

        ' If they select a type of item, set that
        If LoadingList Then
            ComboType = ""
        Else ' We are doing a main query so limit
            If Trim(cmbCalcBPTypeFilter.Text) <> "All Types" And Trim(cmbCalcBPTypeFilter.Text) <> "Select Type" And Trim(cmbCalcBPTypeFilter.Text) <> "" Then
                ComboType = "AND X.ITEM_GROUP ='" & Trim(cmbCalcBPTypeFilter.Text) & "' "
            Else
                ComboType = ""
            End If
        End If

        SizesClause = ""

        ' Finally add the sizes
        If chkCalcSmall.Checked Then ' Light
            SizesClause = SizesClause & "'S',"
        End If

        If chkCalcMedium.Checked Then ' Medium
            SizesClause = SizesClause & "'M',"
        End If

        If chkCalcLarge.Checked Then ' Heavy
            SizesClause = SizesClause & "'L',"
        End If

        If chkCalcXL.Checked Then ' Fighters
            SizesClause = SizesClause & "'XL',"
        End If

        If SizesClause <> "" Then
            SizesClause = " AND SIZE_GROUP IN (" & SizesClause.Substring(0, Len(SizesClause) - 1) & ") "
        End If

        ' Flag for favorites 
        If rbtnCalcBPFavorites.Checked Then
            WhereClause = "WHERE FAVORITE = 1 AND "
        Else
            WhereClause = "WHERE "
        End If

        ' Add all the items to the where clause
        WhereClause = WhereClause & RaceClause & " AND (" & ItemTypes & ") AND (((" & ItemTypeNumbers & ") " & T2Query & T3Query & "))" & SizesClause & ComboType & " "

        ' Finally add on text if they added it
        If Trim(txtCalcItemFilter.Text) <> "" Then
            WhereClause = WhereClause & "AND " & GetSearchText(txtCalcItemFilter.Text, "X.ITEM_NAME", "X.ITEM_GROUP")
        End If

        ' Only bps not ignored - no option for this yet
        WhereClause = WhereClause & " AND IGNORE = 0 "

        Return WhereClause

    End Function

    ' Checks data on different filters to see if we enter the item, and formats colors, etc. after
    Private Sub InsertManufacturingItem(ByVal SentItem As ManufacturingItem, ByVal SVRThreshold As Double,
                                        ByVal InsertBlankSVR As Boolean, ByRef SentList As List(Of ManufacturingItem),
                                        ByRef FormatList As List(Of RowFormat))
        Dim CurrentRowFormat As New RowFormat
        Dim InsertItem As Boolean = True ' Assume we include until the record doesn't pass one condition
        ListIDIterator += 1

        SentItem.ListID = ListIDIterator

        ' If not blank, does it meet the threshold? If nothing, then we want to include it, so skip
        If SentItem.SVR <> "-" And Not IsNothing(SVRThreshold) Then
            ' It's below the threshold, so don't insert
            If CDbl(SentItem.SVR) < SVRThreshold Then
                InsertItem = False
            End If
        End If

        ' If it's empty and you don't want blank svr's, don't insert
        If SentItem.SVR = "-" And Not InsertBlankSVR Then
            InsertItem = False
        End If

        ' Filter based on price trend first
        If cmbCalcPriceTrend.Text = "Up" Then
            ' They want up trends and this is less than zero, so false
            If SentItem.PriceTrend < 0 Then
                InsertItem = False
            End If
        ElseIf cmbCalcPriceTrend.Text = "Down" Then
            ' They want down trends and this is greater than zero, so false
            If SentItem.PriceTrend > 0 Then
                InsertItem = False
            End If
        End If

        ' Min Build time
        If chkCalcMinBuildTimeFilter.Checked Then
            ' If greater than max threshold, don't include
            If ConvertDHMSTimetoSeconds(SentItem.TotalProductionTime) < ConvertDHMSTimetoSeconds(tpMinBuildTimeFilter.Text) Then
                InsertItem = False
            End If
        End If

        ' Max Build time
        If chkCalcMaxBuildTimeFilter.Checked Then
            ' If greater than max threshold, don't include
            If ConvertDHMSTimetoSeconds(SentItem.TotalProductionTime) > ConvertDHMSTimetoSeconds(tpMaxBuildTimeFilter.Text) Then
                InsertItem = False
            End If
        End If

        ' IPH Threshold
        If chkCalcIPHThreshold.Checked Then
            ' If less than threshold, don't include
            If SentItem.IPH < CDbl(txtCalcIPHThreshold.Text) Then
                InsertItem = False
            End If
        End If

        ' Profit Threshold
        If chkCalcProfitThreshold.CheckState = CheckState.Checked Then
            ' If less than threshold, don't include
            If SentItem.Profit < CDbl(txtCalcProfitThreshold.Text) Then
                InsertItem = False
            End If
        ElseIf chkCalcProfitThreshold.CheckState = CheckState.Indeterminate Then
            ' Profit %
            If SentItem.ProfitPercent < CpctD(txtCalcProfitThreshold.Text) Then
                InsertItem = False
            End If
        End If

        ' Profit Threshold
        If chkCalcVolumeThreshold.Checked Then
            ' If less than threshold, don't include
            If SentItem.TotalItemsSold < CDbl(txtCalcVolumeThreshold.Text) Then
                InsertItem = False
            End If
        End If

        ' Now determine the format of the item and save it for drawing the list - only if we add it
        If InsertItem Then
            ' Add the record
            SentList.Add(CType(SentItem.Clone, ManufacturingItem))

            CurrentRowFormat.ListID = ListIDIterator

            'Set the row format for background and foreground colors
            'All columns need to be colored properly
            ' Color owned BP's
            If SentItem.Owned = Yes Then
                If SentItem.Scanned = 1 Or SentItem.Scanned = 0 Then
                    CurrentRowFormat.BackColor = Brushes.BlanchedAlmond
                ElseIf SentItem.Scanned = 2 Then
                    ' Corp owned
                    CurrentRowFormat.BackColor = Brushes.LightGreen
                End If
            ElseIf UserInventedBPs.Contains(SentItem.BPID) Then
                ' It's an invented BP that we own the T1 BP for
                CurrentRowFormat.BackColor = Brushes.LightSkyBlue
            Else
                CurrentRowFormat.BackColor = Brushes.White
            End If

            ' Set default and change if needed
            CurrentRowFormat.ForeColor = Brushes.Black

            ' Highlight those we can't build, RE or Invent
            If Not SentItem.CanBuildBP Then
                CurrentRowFormat.ForeColor = Brushes.DarkRed
            End If

            If Not SentItem.CanInvent And SentItem.TechLevel = "T2" And SentItem.BlueprintType = BPType.InventedBPC And Not chkCalcIgnoreInvention.Checked Then
                CurrentRowFormat.ForeColor = Brushes.DarkOrange
            End If

            If Not SentItem.CanRE And SentItem.TechLevel = "T3" And SentItem.BlueprintType = BPType.InventedBPC And Not chkCalcIgnoreInvention.Checked Then
                CurrentRowFormat.ForeColor = Brushes.DarkGreen
            End If

            ' Insert the format
            FormatList.Add(CurrentRowFormat)

        End If

    End Sub

    ' Checks if the BP is T2 or T3 and we want to save it for determining the optimal calc for decryptors
    Private Sub InsertDecryptorforOptimalCompare(ByRef BP As Blueprint, ByRef CalcType As String, ByRef LocationID As Integer, ByRef OptimalList As List(Of OptimalDecryptorItem))
        If chkCalcDecryptor0.Checked Then
            Dim TempItem As New OptimalDecryptorItem
            Dim CompareIPH As Boolean

            If chkCalcDecryptor0.Text.Contains("Profit") Then
                CompareIPH = False
            Else
                CompareIPH = True
            End If

            ' Insert the record if it has a decryptor and T2/T3
            If (BP.GetTechLevel = BPTechLevel.T2 And chkCalcDecryptorforT2.Checked) Or
               (BP.GetTechLevel = BPTechLevel.T3 And chkCalcDecryptorforT3.Checked) Then
                TempItem.CalcType = CalcType
                TempItem.ItemTypeID = BP.GetItemID
                TempItem.ListLocationID = LocationID

                If CompareIPH Then
                    If CalcType <> "Components" Then
                        TempItem.CompareValue = BP.GetTotalIskperHourRaw
                    Else
                        TempItem.CompareValue = BP.GetTotalIskperHourComponents
                    End If
                Else ' Profit
                    If CalcType <> "Components" Then
                        TempItem.CompareValue = BP.GetTotalRawProfit
                    Else
                        TempItem.CompareValue = BP.GetTotalComponentProfit
                    End If
                End If
            End If

            OptimalList.Add(TempItem)

        End If
    End Sub

    ' Reads optimal decryptor list and removes only but the most optimal decryptor from the item list
    Private Function SetOptimalDecryptorList(ByVal ItemList As List(Of ManufacturingItem), OptimalItemList As List(Of OptimalDecryptorItem)) As List(Of ManufacturingItem)
        Dim TempDecryptorItem As New OptimalDecryptorItem
        Dim TempList As New List(Of OptimalDecryptorItem)
        Dim CompareValue As Double = 0
        Dim OptimalLocationID As Integer = 0
        Dim RemoveLocations As New List(Of Integer)

        If OptimalItemList.Count <> 0 Then
            For i = 0 To ItemList.Count - 1
                ' Get all the items in the decryptor list (if they exist)
                DecryptorItemToFind.CalcType = ItemList(i).CalcType
                DecryptorItemToFind.ItemTypeID = ItemList(i).ItemTypeID
                ' Find all the items
                TempList = OptimalItemList.FindAll(AddressOf FindDecryptorItem)
                If TempList IsNot Nothing Then
                    ' Loop through each one and get the Location ID for the most optimal item
                    For j = 0 To TempList.Count - 1
                        If CompareValue = 0 Or CompareValue <= TempList(j).CompareValue Then
                            CompareValue = TempList(j).CompareValue
                            ' Save/reset the location for the optimal
                            OptimalLocationID = TempList(j).ListLocationID
                        End If
                    Next

                    ' Reset
                    CompareValue = 0

                    ' Insert the location ID into the list to remove later
                    For j = 0 To TempList.Count - 1
                        If TempList(j).ListLocationID <> OptimalLocationID Then
                            ' Remove this one
                            RemoveLocations.Add(TempList(j).ListLocationID)
                        End If
                    Next

                End If
            Next

            ' Finally, remove all the ID's in the remove list
            For i = 0 To RemoveLocations.Count - 1
                ManufacturingRecordIDToFind = RemoveLocations(i)
                ItemList.Remove(ItemList.Find(AddressOf FindManufacturingItem))
            Next
        End If

        Return ItemList

    End Function

    Private DecryptorItemToFind As OptimalDecryptorItem

    ' Predicate for finding an in the list of decryptors
    Private Function FindDecryptorItem(ByVal Item As OptimalDecryptorItem) As Boolean
        If Item.CalcType = DecryptorItemToFind.CalcType And Item.ItemTypeID = DecryptorItemToFind.ItemTypeID Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub lstManufacturing_ColumnWidthChanging(sender As Object, e As System.Windows.Forms.ColumnWidthChangingEventArgs) Handles lstManufacturing.ColumnWidthChanging
        If e.ColumnIndex = 0 Then
            e.Cancel = True
            e.NewWidth = lstPricesView.Columns(e.ColumnIndex).Width
        End If
    End Sub

    ' On double click of the item, it will open up the bp window with the item 
    Private Sub lstManufacturing_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstManufacturing.DoubleClick
        Dim FoundItem As New ManufacturingItem
        Dim CompareType As String

        ' Find the item clicked in the list of items then just send those values over
        ManufacturingRecordIDToFind = CLng(lstManufacturing.SelectedItems(0).SubItems(0).Text)
        FoundItem = FinalManufacturingItemList.Find(AddressOf FindManufacturingItem)

        ' Set the build facility we are sending to the proper facility type for this item. 
        If FoundItem IsNot Nothing Then
            ' We found it, so load the current bp
            With FoundItem
                If rbtnCalcCompareAll.Checked Or rbtnCalcCompareComponents.Checked Or rbtnCalcCompareBuildBuy.Checked Then
                    CompareType = "Components"
                Else
                    CompareType = "Raw"
                End If

                Call LoadBPfromEvent(.BPID, .CalcType, .Inputs, SentFromLocation.ManufacturingTab,
                                     .ManufacturingFacility, .ComponentManufacturingFacility, .CapComponentManufacturingFacility,
                                     .InventionFacility, .CopyFacility,
                                     chkCalcTaxes.Checked, chkCalcFees.Checked,
                                     CStr(.BPME), CStr(.BPTE), txtCalcRuns.Text, txtCalcProdLines.Text, txtCalcLabLines.Text,
                                     txtCalcNumBPs.Text, FormatNumber(.AddlCosts, 2), chkCalcPPU.Checked, CompareType)
            End With
        End If

    End Sub

    ' The manufacturing item to load the grid
    Public Class ManufacturingItem
        Implements ICloneable

        Public ListID As Integer ' Unique record id

        Public Blueprint As Blueprint ' The blueprint we used to make this item - for shopping list references

        Public BPID As Long
        Public ItemGroup As String
        Public ItemGroupID As Integer
        Public ItemCategory As String
        Public ItemCategoryID As Integer
        Public ItemTypeID As Long
        Public ItemName As String
        Public TechLevel As String
        Public Owned As String
        Public Scanned As Integer
        Public BPME As Integer
        Public BPTE As Integer
        Public Inputs As String
        Public AddlCosts As Double
        Public Profit As Double
        Public ProfitPercent As Double
        Public IPH As Double
        Public TotalCost As Double
        Public CalcType As String ' Type of calculation to get the profit - either Components, Raw Mats or Build/Buy
        Public BlueprintType As BPType

        Public Runs As Integer
        Public SingleInventedBPCRunsperBPC As Integer
        Public ProductionLines As Integer
        Public LaboratoryLines As Integer

        ' Inputs
        Public Decryptor As New Decryptor
        Public Relic As String
        Public SavedBPRuns As Integer ' The number of runs on the bp that they have, helpful for determing decryptor and relics

        ' Can do variables
        Public CanBuildBP As Boolean
        Public CanInvent As Boolean
        Public CanRE As Boolean

        Public SVR As String ' Sales volume ratio
        Public SVRxIPH As String
        Public PriceTrend As Double
        Public TotalItemsSold As Long
        Public TotalOrdersFilled As Long
        Public AvgItemsperOrder As Double
        Public CurrentSellOrders As Long
        Public CurrentBuyOrders As Long
        Public ItemsinStock As Integer
        Public ItemsinProduction As Integer

        Public ManufacturingFacility As IndustryFacility
        Public ManufacturingFacilityUsage As Double
        Public ComponentManufacturingFacility As IndustryFacility
        Public ComponentManufacturingFacilityUsage As Double
        Public CapComponentManufacturingFacility As IndustryFacility
        Public CapComponentManufacturingFacilityUsage As Double

        Public CopyCost As Double
        Public CopyFacilityUsage As Double
        Public CopyFacility As IndustryFacility

        Public InventionCost As Double
        Public InventionFacilityUsage As Double
        Public InventionFacility As IndustryFacility

        Public BPProductionTime As String
        Public TotalProductionTime As String
        Public CopyTime As String
        Public InventionTime As String

        Public ItemMarketPrice As Double

        Public BrokerFees As Double
        Public Taxes As Double

        Public BaseJobCost As Double
        Public NumBPs As Integer
        Public InventionChance As Double
        Public Race As String
        Public VolumeperItem As Double
        Public TotalVolume As Double
        Public PortionSize As Integer
        Public DivideUnits As Integer

        Public JobFee As Double

        Public Function Clone() As Object Implements System.ICloneable.Clone
            Dim CopyofMe As New ManufacturingItem

            CopyofMe.ListID = ListID
            CopyofMe.Blueprint = Blueprint
            CopyofMe.BPID = BPID
            CopyofMe.ItemGroup = ItemGroup
            CopyofMe.ItemGroupID = ItemGroupID
            CopyofMe.ItemCategory = ItemCategory
            CopyofMe.ItemCategoryID = ItemCategoryID
            CopyofMe.ItemTypeID = ItemTypeID
            CopyofMe.ItemName = ItemName
            CopyofMe.TechLevel = TechLevel
            CopyofMe.Owned = Owned
            CopyofMe.Scanned = Scanned
            CopyofMe.BPME = BPME
            CopyofMe.BPTE = BPTE
            CopyofMe.Inputs = Inputs
            CopyofMe.AddlCosts = AddlCosts
            CopyofMe.Profit = Profit
            CopyofMe.ProfitPercent = ProfitPercent
            CopyofMe.IPH = IPH
            CopyofMe.TotalCost = TotalCost
            CopyofMe.CalcType = CalcType
            CopyofMe.BlueprintType = BlueprintType

            CopyofMe.Runs = Runs
            CopyofMe.SingleInventedBPCRunsperBPC = SingleInventedBPCRunsperBPC
            CopyofMe.ProductionLines = ProductionLines
            CopyofMe.LaboratoryLines = LaboratoryLines

            CopyofMe.CopyTime = CopyTime
            CopyofMe.InventionTime = InventionTime

            CopyofMe.Inputs = Inputs
            CopyofMe.Decryptor = Decryptor
            CopyofMe.Relic = Relic
            CopyofMe.SavedBPRuns = SavedBPRuns

            CopyofMe.CanBuildBP = CanBuildBP
            CopyofMe.CanInvent = CanInvent
            CopyofMe.CanRE = CanRE

            CopyofMe.SVR = SVR
            CopyofMe.SVRxIPH = SVRxIPH
            CopyofMe.PriceTrend = PriceTrend
            CopyofMe.TotalItemsSold = TotalItemsSold
            CopyofMe.TotalOrdersFilled = TotalOrdersFilled
            CopyofMe.AvgItemsperOrder = AvgItemsperOrder
            CopyofMe.CurrentSellOrders = CurrentSellOrders
            CopyofMe.CurrentBuyOrders = CurrentBuyOrders
            CopyofMe.ItemsinStock = ItemsinStock
            CopyofMe.ItemsinProduction = ItemsinProduction

            CopyofMe.CopyCost = CopyCost
            CopyofMe.InventionCost = InventionCost
            CopyofMe.ManufacturingFacilityUsage = ManufacturingFacilityUsage

            CopyofMe.ManufacturingFacility = ManufacturingFacility
            CopyofMe.ComponentManufacturingFacility = ComponentManufacturingFacility
            CopyofMe.CapComponentManufacturingFacility = CapComponentManufacturingFacility
            CopyofMe.InventionFacility = InventionFacility
            CopyofMe.CopyFacility = CopyFacility

            CopyofMe.BPProductionTime = BPProductionTime
            CopyofMe.TotalProductionTime = TotalProductionTime
            CopyofMe.ItemMarketPrice = ItemMarketPrice
            CopyofMe.BrokerFees = BrokerFees
            CopyofMe.Taxes = Taxes
            CopyofMe.BaseJobCost = BaseJobCost

            CopyofMe.NumBPs = NumBPs
            CopyofMe.InventionChance = InventionChance
            CopyofMe.BlueprintType = BlueprintType
            CopyofMe.Race = Race
            CopyofMe.VolumeperItem = VolumeperItem
            CopyofMe.TotalVolume = TotalVolume
            CopyofMe.PortionSize = PortionSize
            CopyofMe.DivideUnits = DivideUnits

            CopyofMe.JobFee = JobFee

            CopyofMe.ManufacturingFacilityUsage = ManufacturingFacilityUsage
            CopyofMe.ComponentManufacturingFacilityUsage = ComponentManufacturingFacilityUsage
            CopyofMe.CapComponentManufacturingFacilityUsage = CapComponentManufacturingFacilityUsage
            CopyofMe.CopyFacilityUsage = CopyFacilityUsage
            CopyofMe.InventionFacilityUsage = InventionFacilityUsage

            Return CopyofMe

        End Function

    End Class

    ' Predicate for finding an item in a list EVE Market Data of items
    Private Function FindManufacturingItem(ByVal Item As ManufacturingItem) As Boolean
        If Item.ListID = ManufacturingRecordIDToFind Then
            Return True
        Else
            Return False
        End If
    End Function

    ' Predicate for finding an item in a list EVE Market Data of items
    Private Function FindManufacturingItembyName(ByVal Item As ManufacturingItem) As Boolean
        If Item.ItemName = ManufacturingNameToFind Then
            Return True
        Else
            Return False
        End If
    End Function

    ' Calculates the slope of the trend line for the market history for the sent type id for the last x days sent
    ' Formula and logic from here: http://classroom.synonym.com/calculate-trendline-2709.html
    Private Function CalculatePriceTrend(ByVal TypeID As Long, ByVal RegionID As Long, DaysfromToday As Integer) As Double
        Dim SQL As String
        Dim rsMarketHistory As SQLiteDataReader
        Dim GraphData As New List(Of EVEIPHPricePoint)
        Dim counter As Integer = 0

        Dim n_value As Double = 0 ' Let n = the number of data points, in this case 3
        Dim a_value As Double = 0
        Dim b_value As Double = 0
        Dim c_value As Double = 0
        Dim d_value As Double = 0
        Dim e_value As Double = 0
        Dim f_value As Double = 0

        Dim x_sum As Double = 0
        Dim x_squared As Double = 0
        Dim y_sum As Double = 0

        Dim slope As Double = 0
        Dim y_intercept As Double = 0

        Dim AdjustPrice As Double = 0

        ' Average price is the Y values, dates (or just days) is the x value

        ' Now get all the prices for the time period
        SQL = "SELECT PRICE_HISTORY_DATE, AVG_PRICE FROM MARKET_HISTORY WHERE TYPE_ID = " & CStr(TypeID) & " AND REGION_ID = " & CStr(RegionID) & " "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) >= " & " DateTime('" & Format(DateAdd(DateInterval.Day, -(DaysfromToday + 1), Now.Date), SQLiteDateFormat) & "') "
        SQL = SQL & "AND DATETIME(PRICE_HISTORY_DATE) < " & " DateTime('" & Format(Now.Date, SQLiteDateFormat) & "') "
        SQL = SQL & "ORDER BY PRICE_HISTORY_DATE ASC"
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        rsMarketHistory = DBCommand.ExecuteReader

        While rsMarketHistory.Read
            Dim TempPoint As EVEIPHPricePoint
            counter += 1
            TempPoint.PointDate = rsMarketHistory.GetDateTime(0)
            TempPoint.X_Date_Marker = counter
            If counter = 1 Then
                ' Save the base value and then reduce from all other prices
                AdjustPrice = rsMarketHistory.GetDouble(1)
                TempPoint.Y_Price = rsMarketHistory.GetDouble(1)
            Else
                TempPoint.Y_Price = rsMarketHistory.GetDouble(1)
            End If

            ' Since we are looping through the data, just do the summation calcs now
            a_value += (counter * TempPoint.Y_Price)
            ' Grab the sum here too
            y_sum += TempPoint.Y_Price
            x_sum += TempPoint.X_Date_Marker
            x_squared += TempPoint.X_Date_Marker ^ 2

            GraphData.Add(TempPoint)
        End While

        ' Set the n_value from the loop
        If counter <= 1 Then
            ' If it's 0 or 1, then we can't do a slope calculation 
            Return 0
        Else
            n_value = counter
        End If

        ' Now we have all the data to do the calculations

        ' Calculate a
        ' Let a equal n times the summation of all x-values multiplied by their corresponding y-values, like so: a = 3 x {(1 x 3) +( 2 x 5) + (3 x 6.5)} = 97.5
        ' Use previous calc value and multiply by n
        a_value = a_value * n_value

        ' Calculate b
        ' Let b equal the sum of all x-values times the sum of all y-values, like so: b = (1 + 2 + 3) x (3 + 5 + 6.5) = 87
        ' Use x_sum and y_sum from earlier and calculate b
        b_value = x_sum * y_sum

        ' Calculate c
        ' Let c equal n times the sum of all squared x-values, like so: c = 3 x (1^2 + 2^2 + 3^2) = 42
        c_value = n_value * x_squared

        ' Calculate d
        ' Let d equal the squared sum of all x-values, like so: d = (1 + 2 + 3)^2 = 36
        d_value = x_sum ^ 2

        ' Calculate the slope
        ' Plug the values that you calculated for a, b, c, and d into the following equation to calculate the slope, m, of the regression line: 
        ' slope = m = (a - b) / (c - d) = (97.5 - 87) / (42 - 36) = 10.5 / 6 = 1.75
        slope = (a_value - b_value) / (c_value - d_value)

        ' Now find the intercepts so we can normalize the slope value
        ' Consider the same data set. Let e equal the sum of all y-values, like so: e = (3 + 5 + 6.5) = 14.5
        e_value = y_sum

        ' Let f equal the slope times the sum of all x-values, like so: f = 1.75 x (1 + 2 + 3) = 10.5
        f_value = slope * x_sum

        ' Calculate the y-intercept
        ' Plug the values you have calculated for e and f into the following equation for the y-intercept, b, of the trendline: 
        ' y-intercept = b = (e - f) / n = (14.5 - 10.5) / 3 = 1.3)
        y_intercept = (e_value - f_value) / n_value

        ' Now that we have all the parts of y = mx + b, normalize the trendline to a percentage change value
        ' First figure out the value today (the start value is the y-intercept)
        Dim TodaysTrendLinePrice As Double = slope * n_value + y_intercept
        'y = 50,098.90x - 1,518,343.83
        Dim trend As Double = (TodaysTrendLinePrice - y_intercept) / TodaysTrendLinePrice
        Return trend

    End Function

    Public Structure EVEIPHPricePoint
        Dim PointDate As Date
        Dim X_Date_Marker As Integer ' simplifies code for dates
        Dim Y_Price As Double ' price value
    End Structure

#Region "List Options Menu"

    Private Sub ListOptionsMenu_Opening(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles ListOptionsMenu.Opening
        ' If we have one line selected, then allow both options, if more than one line don't allow the market history to be selected
        If lstManufacturing.SelectedItems.Count > 1 Then
            ViewMarketHistoryToolStripMenuItem.Enabled = False
        Else
            ViewMarketHistoryToolStripMenuItem.Enabled = True
        End If
    End Sub

    ' Allows users to ignore one or more blueprints from the manufacturing tab
    Private Sub IgnoreBlueprintToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles IgnoreBlueprintToolStripMenuItem.Click

        If lstManufacturing.Items.Count > 0 Then
            Dim FoundItem As New ManufacturingItem
            Dim SQL As String
            Dim RemovedIDs As New List(Of Integer)

            ' Find the each item selected in the list of items then remove each one from the list
            For i = 0 To lstManufacturing.SelectedItems.Count - 1
                ManufacturingRecordIDToFind = CLng(lstManufacturing.SelectedItems(i).SubItems(0).Text)
                FoundItem = FinalManufacturingItemList.Find(AddressOf FindManufacturingItem)

                If FoundItem IsNot Nothing Then
                    Dim ListIDstoRemove As New List(Of Integer)

                    ' We found it, so set the bp to ignore
                    With FoundItem
                        SQL = "UPDATE ALL_BLUEPRINTS SET IGNORE = 1 WHERE BLUEPRINT_ID = " & CStr(FoundItem.BPID)
                        Call EVEDB.ExecuteNonQuerySQL(SQL)

                        ' Remove the item from the list in all it's forms plus from the manufacturing list
                        ' Get all the items with the name to remove
                        ManufacturingNameToFind = FoundItem.ItemName
                        FoundItem = Nothing

                        Do
                            FoundItem = FinalManufacturingItemList.Find(AddressOf FindManufacturingItembyName)
                            If FoundItem IsNot Nothing Then
                                ' Remove it
                                FinalManufacturingItemList.Remove(FoundItem)
                                RemovedIDs.Add(FoundItem.ListID)
                            End If
                        Loop Until FoundItem Is Nothing

                    End With
                End If
            Next

            ' Now remove all BPs we got rid of from the list
            lstManufacturing.BeginUpdate()
            Dim ListCount As Integer = lstManufacturing.Items.Count
            Dim j As Integer = 0
            While j < ListCount
                If RemovedIDs.Contains(CInt(lstManufacturing.Items(j).SubItems(0).Text)) Then
                    ' Add the indicies to remove
                    lstManufacturing.Items(j).Remove()
                    ListCount -= 1
                    j -= 1 ' make sure we reset since we just removed a line
                End If
                j += 1
            End While

            lstManufacturing.EndUpdate()

            Call PlayNotifySound()
        End If
    End Sub

    ' Gets the typeID and other data to open up the market history viewer
    Private Sub ViewMarketHistoryToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ViewMarketHistoryToolStripMenuItem.Click
        Dim FoundItem As New ManufacturingItem
        Dim RegionID As Long

        ' Find the item clicked in the list of items by looking up the row number stored in the hidden column
        If lstManufacturing.Items.Count > 0 And lstManufacturing.SelectedItems.Count > 0 Then
            ManufacturingRecordIDToFind = CLng(lstManufacturing.SelectedItems(0).SubItems(0).Text)
            FoundItem = FinalManufacturingItemList.Find(AddressOf FindManufacturingItem)

            If FoundItem IsNot Nothing Then
                ' Get the region ID
                RegionID = GetRegionID(cmbCalcHistoryRegion.Text)
                If RegionID = 0 Then
                    RegionID = TheForgeTypeID
                End If

                Dim f1 As New frmMarketHistoryViewer(FoundItem.ItemTypeID, FoundItem.ItemName, RegionID, cmbCalcHistoryRegion.Text,
                                                     CInt(cmbCalcAvgPriceDuration.Text))
                f1.Show()

            Else
                MsgBox("Unable to find item data for history", vbInformation, Application.ProductName)
            End If
        End If
    End Sub

    ' Adds one or multiple items to the shopping list from the manufacturing tab
    Private Sub AddToShoppingListToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles AddToShoppingListToolStripMenuItem.Click

        If lstManufacturing.Items.Count > 0 Then
            Dim FoundItem As New ManufacturingItem

            ' Find the each item selected in the list of items then remove each one from the list
            For i = 0 To lstManufacturing.SelectedItems.Count - 1

                ' Find the item clicked in the list of items then just send those values over
                ManufacturingRecordIDToFind = CLng(lstManufacturing.SelectedItems(i).SubItems(0).Text)
                FoundItem = FinalManufacturingItemList.Find(AddressOf FindManufacturingItem)

                ' Add it to shopping list
                If FoundItem IsNot Nothing Then
                    Dim BuildBuy As Boolean
                    Dim CopyRaw As Boolean

                    If FoundItem.CalcType = "Build/Buy" Then
                        BuildBuy = True
                    End If

                    If FoundItem.CalcType = "Raw" Or BuildBuy = True Then
                        CopyRaw = True
                    Else
                        CopyRaw = False
                    End If

                    ' Get the BP variable and send the other settings to shopping list
                    With FoundItem
                        If Not IsNothing(.Blueprint) Then
                            'Call AddToShoppingList(.Blueprint, BuildBuy, CopyRaw, .Blueprint.GetManufacturingFacility.MaterialMultiplier,
                            '                   .Blueprint.GetManufacturingFacility.FacilityType,
                            '                   chkCalcIgnoreInvention.Checked, chkCalcIgnoreMinerals.Checked, chkCalcIgnoreT1Item.Checked,
                            '                   .Blueprint.GetManufacturingFacility.IncludeActivityCost, .Blueprint.GetManufacturingFacility.IncludeActivityTime,
                            '                   .Blueprint.GetManufacturingFacility.IncludeActivityUsage)
                        Else
                            MsgBox("You must calculate an item before adding it to the shopping list.", MsgBoxStyle.Information, Application.ProductName)
                            Exit Sub
                        End If
                    End With
                End If
            Next
        End If

        If TotalShoppingList.GetNumShoppingItems > 0 Then
            ' Add the final item and mark as items in list
            pnlShoppingList.Text = "Items in Shopping List"
            pnlShoppingList.ForeColor = Color.Red
        Else
            pnlShoppingList.Text = "No Items in Shopping List"
            pnlShoppingList.ForeColor = Color.Black
        End If

        ' Refresh the data if it's open
        If frmShop.Visible Then
            Call frmShop.RefreshLists()
        End If

    End Sub

#End Region

#End Region

#Region "Datacores"

#Region "Datacores Tab User Object (Check boxes, Text, Buttons) Functions/Procedures "

    Private Sub CorpCheckBoxOnClickLabel(ByVal index As Integer)
        If DCCorpCheckBoxes(index).Checked Then
            DCCorpCheckBoxes(index).Checked = False
        Else
            DCCorpCheckBoxes(index).Checked = True
        End If
    End Sub

    Private Sub CoreCheckBoxOnClickLabel(ByVal index As Integer)
        If DCSkillCheckBoxes(index).Checked Then
            DCSkillCheckBoxes(index).Checked = False
        Else
            DCSkillCheckBoxes(index).Checked = True
        End If
    End Sub

    Private Sub lblDCCorp1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp1.Click
        Call CorpCheckBoxOnClickLabel(1)
    End Sub

    Private Sub lblDCCorp2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp2.Click
        Call CorpCheckBoxOnClickLabel(2)
    End Sub

    Private Sub lblDCCorp3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp3.Click
        Call CorpCheckBoxOnClickLabel(3)
    End Sub

    Private Sub lblDCCorp4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp4.Click
        Call CorpCheckBoxOnClickLabel(4)
    End Sub

    Private Sub lblDCCorp5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp5.Click
        Call CorpCheckBoxOnClickLabel(5)
    End Sub

    Private Sub lblDCCorp6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp6.Click
        Call CorpCheckBoxOnClickLabel(6)
    End Sub

    Private Sub lblDCCorp7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp7.Click
        Call CorpCheckBoxOnClickLabel(7)
    End Sub

    Private Sub lblDCCorp8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp8.Click
        Call CorpCheckBoxOnClickLabel(8)
    End Sub

    Private Sub lblDCCorp9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp9.Click
        Call CorpCheckBoxOnClickLabel(9)
    End Sub

    Private Sub lblDCCorp10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp10.Click
        Call CorpCheckBoxOnClickLabel(10)
    End Sub

    Private Sub lblDCCorp11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp11.Click
        Call CorpCheckBoxOnClickLabel(11)
    End Sub

    Private Sub lblDCCorp12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp12.Click
        Call CorpCheckBoxOnClickLabel(12)
    End Sub

    Private Sub lblDCCorp13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDCCorp13.Click
        Call CorpCheckBoxOnClickLabel(13)
    End Sub

    Private Sub lblDatacore1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore1.Click
        Call CoreCheckBoxOnClickLabel(1)
    End Sub

    Private Sub lblDatacore2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore2.Click
        Call CoreCheckBoxOnClickLabel(2)
    End Sub

    Private Sub lblDatacore3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore3.Click
        Call CoreCheckBoxOnClickLabel(3)
    End Sub

    Private Sub lblDatacore4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore4.Click
        Call CoreCheckBoxOnClickLabel(4)
    End Sub

    Private Sub lblDatacore5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore5.Click
        Call CoreCheckBoxOnClickLabel(5)
    End Sub

    Private Sub lblDatacore6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore6.Click
        Call CoreCheckBoxOnClickLabel(6)
    End Sub

    Private Sub lblDatacore7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore7.Click
        Call CoreCheckBoxOnClickLabel(7)
    End Sub

    Private Sub lblDatacore8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore8.Click
        Call CoreCheckBoxOnClickLabel(8)
    End Sub

    Private Sub lblDatacore9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore9.Click
        Call CoreCheckBoxOnClickLabel(9)
    End Sub

    Private Sub lblDatacore10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore10.Click
        Call CoreCheckBoxOnClickLabel(10)
    End Sub

    Private Sub lblDatacore11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore11.Click
        Call CoreCheckBoxOnClickLabel(11)
    End Sub

    Private Sub lblDatacore12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore12.Click
        Call CoreCheckBoxOnClickLabel(12)
    End Sub

    Private Sub lblDatacore13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore13.Click
        Call CoreCheckBoxOnClickLabel(13)
    End Sub

    Private Sub lblDatacore14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore14.Click
        Call CoreCheckBoxOnClickLabel(14)
    End Sub

    Private Sub lblDatacore15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore15.Click
        Call CoreCheckBoxOnClickLabel(15)
    End Sub

    Private Sub lblDatacore16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore16.Click
        Call CoreCheckBoxOnClickLabel(16)
    End Sub

    Private Sub lblDatacore17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblDatacore17.Click
        Call CoreCheckBoxOnClickLabel(17)
    End Sub

    Private Sub lstDC_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lstDC.ItemCheck
        Dim TotalAgents As Integer = CInt(cmbDCResearchMgmt.Text) + 1

        If TotalAgents = lstDC.CheckedItems.Count And e.NewValue = CheckState.Checked Then
            ' Change to unchecked
            e.NewValue = CheckState.Unchecked
        End If

    End Sub

    Private Sub lstDC_ItemChecked(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) Handles lstDC.ItemChecked

        ' Item was checked so add up the total iph
        If e.Item.Checked Then
            ' Add the value 
            TotalSelectedIPH = TotalSelectedIPH + CDbl(e.Item.SubItems(DCIPH_COLUMN).Text)
        ElseIf Not e.Item.Checked Then
            If lstDC.CheckedItems.Count = 0 Then
                ' Reset if last one checked
                TotalSelectedIPH = 0
            Else
                ' Subtract the amount
                TotalSelectedIPH = TotalSelectedIPH - CDbl(e.Item.SubItems(DCIPH_COLUMN).Text)
            End If
        End If

        txtDCTotalSelectedIPH.Text = CStr(FormatNumber(TotalSelectedIPH, 2))

    End Sub

    Private Sub cmbDCConnections_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbDCConnections.SelectedIndexChanged
        Call LoadDCCorpStandings(False)
    End Sub

    Private Sub chkDC1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC1.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC2.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC3.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC4.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC5.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC6.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC7_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC7.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC8_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC8.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC9_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC9.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC10_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC10.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC11_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC11.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC12_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC12.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC13_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC13.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC14_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC14.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC15_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC15.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC16_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC16.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDC17_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDC17.CheckedChanged
        Call EnableDCSkillChecks(sender)
    End Sub

    Private Sub chkDCCorp1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp1.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp2.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp3.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp4.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp5.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp6.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp7_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp7.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp8_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp8.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp9_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp9.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp10_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp10.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp11_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp11.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp12_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp12.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub chkDCCorp13_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDCCorp13.CheckedChanged
        Call EnableDCCorpTexts(sender)
    End Sub

    Private Sub EnableDCSkillChecks(ByVal sender As Object)
        Dim i As Integer
        Dim cbox As CheckBox

        cbox = DirectCast(sender, CheckBox)

        If Not FirstShowDatacores Then
            For i = 1 To DCSkillCheckBoxes.Count - 1
                If cbox.Name = DCSkillCheckBoxes(i).Name Then
                    If CBool(cbox.Checked) Then
                        DCSkillCombos(i).Enabled = True
                    Else
                        DCSkillCombos(i).Enabled = False
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub EnableDCCorpTexts(ByVal sender As Object)
        Dim i As Integer
        Dim cbox As CheckBox

        cbox = DirectCast(sender, CheckBox)

        If Not FirstShowDatacores Then
            For i = 1 To DCCorpCheckBoxes.Count - 1
                If cbox.Name = DCCorpCheckBoxes(i).Name Then
                    If CBool(cbox.Checked) Then
                        DCCorpTextboxes(i).Enabled = True
                    Else
                        DCCorpTextboxes(i).Enabled = False
                    End If

                End If
            Next
        End If
    End Sub

    Private Sub btnDCReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDCReset.Click
        Call LoadDatacoreTab()
    End Sub

    Private Sub lstDC_ColumnClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstDC.ColumnClick
        Call ListViewColumnSorter(e.Column, lstDC, DCColumnClicked, DCColumnSortType)
    End Sub

#End Region

    Public Structure DCAgent
        Dim Faction As String
        Dim FactionStanding As Double
        Dim Corporation As String
        Dim CorporationStanding As Double
        Dim Agent As String
        Dim AgentStanding As Double
        Dim AgentLevel As Integer
        'Dim AgentQuality As Integer *** Removed on 19 May 2011 ***
        Dim AgentLocation As String ' System name + security
        Dim DataCoreID As Long
        Dim DataCoreSkill As String
        Dim DataCoreSkillLevel As Integer
        Dim DataCorePrice As Double
        Dim PriceFrom As String
        Dim RPperDay As Double
        Dim CoresPerDay As Double
        Dim IskPerHour As Double
        ' Location ID's
        Dim SystemID As Long
        Dim SystemSecurity As Double
        Dim RegionID As Long

        Dim AgentAvailable As Boolean

    End Structure

    Private Sub InitDatacoreTab()
        ' Reload screen when called
        Call LoadDatacoreTab()
    End Sub

    ' Loads the datacore skills into the Datacore screen
    Private Sub LoadDatacoreTab()
        Dim i As Integer
        Dim TempSkillLevel As Integer
        Dim Settings As New ProgramSettings

        ' Load the datacore skills first
        For i = 1 To DCSkillLabels.Count - 1
            TempSkillLevel = SelectedCharacter.Skills.GetSkillLevel(SelectedCharacter.Skills.GetSkillTypeID(DCSkillLabels(i).Text))

            ' Check based on default
            If UserDCTabSettings.SkillsChecked(i - 1) = Settings.DefaultSkillLevelChecked Then
                If TempSkillLevel <> 0 Then
                    ' Check
                    DCSkillCheckBoxes(i).Checked = True
                    DCSkillCombos(i).Enabled = True
                Else
                    DCSkillCombos(i).Text = "1"
                    DCSkillCheckBoxes(i).Checked = False
                    DCSkillCombos(i).Enabled = False
                End If
            Else ' use what they saved
                DCSkillCombos(i).Text = "1"
                DCSkillCheckBoxes(i).Checked = CBool(UserDCTabSettings.SkillsChecked(i - 1))
                DCSkillCombos(i).Enabled = CBool(UserDCTabSettings.SkillsChecked(i - 1))
            End If

            ' Use the default or use what they saved
            If UserDCTabSettings.SkillsLevel(i - 1) = Settings.DefaultSkillLevel Then
                DCSkillCombos(i).Text = CStr(TempSkillLevel)
            Else ' use what they saved
                DCSkillCombos(i).Text = CStr(UserDCTabSettings.SkillsLevel(i - 1))
            End If

        Next

        ' Load the connections and negotiation skill. If default, then load skills else use what they set
        If UserDCTabSettings.Connections = Settings.DefaultConnections Then
            cmbDCConnections.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(3359))
        Else
            cmbDCConnections.Text = CStr(UserDCTabSettings.Connections)
        End If

        If UserDCTabSettings.Negotiation = Settings.DefaultNegotiation Then
            cmbDCNegotiation.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(3356))
        Else
            cmbDCNegotiation.Text = CStr(UserDCTabSettings.Negotiation)
        End If

        If UserDCTabSettings.ResearchProjectMgt = Settings.DefaultResearchProjMgt Then
            cmbDCResearchMgmt.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(12179))
        Else
            cmbDCResearchMgmt.Text = CStr(UserDCTabSettings.ResearchProjectMgt)
        End If

        ' Load the corp standing text boxes
        Call LoadDCCorpStandings()

        ' Check the Corporation Standings boxes if not 0
        For i = 1 To DCCorpLabels.Count - 1
            If DCCorpTextboxes(i).Text <> "0.00" Then
                DCCorpCheckBoxes(i).Checked = True
                DCCorpTextboxes(i).Enabled = True
            Else
                DCCorpCheckBoxes(i).Checked = False
                DCCorpTextboxes(i).Enabled = False
            End If
        Next

        Select Case UserDCTabSettings.PricesFrom
            Case rbtnDCUpdatedPrices.Text
                rbtnDCUpdatedPrices.Checked = True
            Case rbtnDCRegionPrices.Text
                rbtnDCRegionPrices.Checked = True
            Case rbtnDCSystemPrices.Text
                rbtnDCSystemPrices.Checked = True
        End Select

        chkDCHighSecAgents.Checked = UserDCTabSettings.CheckHighSecAgents
        chkDCLowSecAgents.Checked = UserDCTabSettings.CheckLowNullSecAgents
        chkDCIncludeAllAgents.Checked = UserDCTabSettings.CheckIncludeAgentsCannotAccess

        ' Sov checks
        chkDCAmarrSov.Checked = UserDCTabSettings.CheckSovAmarr
        chkDCAmmatarSov.Checked = UserDCTabSettings.CheckSovAmmatar
        chkDCCaldariSov.Checked = UserDCTabSettings.CheckSovCaldari
        chkDCGallenteSov.Checked = UserDCTabSettings.CheckSovGallente
        chkDCKhanidSov.Checked = UserDCTabSettings.CheckSovKhanid
        chkDCMinmatarSov.Checked = UserDCTabSettings.CheckSovMinmatar
        chkDCSyndicateSov.Checked = UserDCTabSettings.CheckSovSyndicate
        chkDCThukkerSov.Checked = UserDCTabSettings.CheckSovThukker

        DCColumnClicked = UserDCTabSettings.ColumnSort
        If UserDCTabSettings.ColumnSortType = "Ascending" Then
            DCColumnSortType = SortOrder.Ascending
        Else
            DCColumnSortType = SortOrder.Descending
        End If

        cmbDCRegions.Text = UserDCTabSettings.AgentsInRegion

    End Sub

    Private Sub btnDCSaveSettings_Click(sender As System.Object, e As System.EventArgs) Handles btnDCSaveSettings.Click
        Dim TempSettings As DataCoreTabSettings = Nothing
        Dim Settings As New ProgramSettings
        Dim TempSkill As Integer
        Dim TempStanding As Double

        ReDim TempSettings.SkillsLevel(Settings.NumberofDCSettingsSkillRecords)
        ReDim TempSettings.SkillsChecked(Settings.NumberofDCSettingsSkillRecords)
        ReDim TempSettings.CorpsStanding(Settings.NumberofDCSettingsCorpRecords)
        ReDim TempSettings.CorpsChecked(Settings.NumberofDCSettingsCorpRecords)

        If rbtnDCUpdatedPrices.Checked = True Then
            TempSettings.PricesFrom = rbtnDCUpdatedPrices.Text
        ElseIf rbtnDCRegionPrices.Checked = True Then
            TempSettings.PricesFrom = rbtnDCRegionPrices.Text
        ElseIf rbtnDCSystemPrices.Checked = True Then
            TempSettings.PricesFrom = rbtnDCSystemPrices.Text
        End If

        TempSettings.CheckHighSecAgents = chkDCHighSecAgents.Checked
        TempSettings.CheckLowNullSecAgents = chkDCLowSecAgents.Checked
        TempSettings.CheckIncludeAgentsCannotAccess = chkDCIncludeAllAgents.Checked

        TempSettings.CheckSovAmarr = chkDCAmarrSov.Checked
        TempSettings.CheckSovAmmatar = chkDCAmmatarSov.Checked
        TempSettings.CheckSovCaldari = chkDCCaldariSov.Checked
        TempSettings.CheckSovGallente = chkDCGallenteSov.Checked
        TempSettings.CheckSovKhanid = chkDCKhanidSov.Checked
        TempSettings.CheckSovMinmatar = chkDCMinmatarSov.Checked
        TempSettings.CheckSovSyndicate = chkDCSyndicateSov.Checked
        TempSettings.CheckSovThukker = chkDCThukkerSov.Checked

        TempSettings.AgentsInRegion = cmbDCRegions.Text

        ' Save skills
        For i = 1 To DCSkillCheckBoxes.Count - 1
            TempSkill = SelectedCharacter.Skills.GetSkillLevel(SelectedCharacter.Skills.GetSkillTypeID(DCSkillLabels(i).Text))

            ' Only save if they don't have the skill and have checked it or have it and unchecked it
            If (TempSkill = 0 And DCSkillCheckBoxes(i).Checked = False) Or (TempSkill <> 0 And DCSkillCheckBoxes(i).Checked = True) Then
                ' Save as default
                TempSettings.SkillsChecked(i - 1) = Settings.DefaultSkillLevelChecked
            Else
                ' Got a value
                TempSettings.SkillsChecked(i - 1) = CInt(DCSkillCheckBoxes(i).Checked)
            End If

            ' If the skill level they have is the same as the skill of the character, then just save as default
            If CInt(DCSkillCombos(i).Text) = TempSkill Then
                TempSettings.SkillsLevel(i - 1) = Settings.DefaultSkillLevel
            Else
                TempSettings.SkillsLevel(i - 1) = CInt(DCSkillCombos(i).Text)
            End If
        Next

        ' Save Corp Standings
        For i = 1 To DCCorpCheckBoxes.Count - 1
            TempStanding = CDbl(FormatNumber(SelectedCharacter.Standings.GetEffectiveStanding(DCCorpLabels(i).Text, CInt(cmbDCConnections.Text), SelectedCharacter.Skills.GetSkillLevel(3357)), 2))

            ' Only save if they don't have the standing and it's checked or they have it and unchecked
            If (TempStanding = 0 And DCCorpCheckBoxes(i).Checked = False) Or (TempStanding <> 0 And DCCorpCheckBoxes(i).Checked = True) Then
                TempSettings.CorpsChecked(i - 1) = Settings.DefaultCorpStandingChecked
            Else
                TempSettings.CorpsChecked(i - 1) = CInt(DCCorpCheckBoxes(i).Checked)
            End If

            ' If SetWindowTheme standing level they have is the same as the standing on the characters, just save as default
            If CDbl(DCCorpTextboxes(i).Text) = TempStanding Then
                TempSettings.CorpsStanding(i - 1) = Settings.DefaultCorpStanding
            Else
                TempSettings.CorpsStanding(i - 1) = CDbl(DCCorpTextboxes(i).Text)
            End If
        Next

        ' Three main skills, only save if they aren't the same as the character
        If cmbDCConnections.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(3359)) Then
            TempSettings.Connections = Settings.DefaultConnections
        Else
            TempSettings.Connections = CInt(cmbDCConnections.Text)
        End If

        If cmbDCNegotiation.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(3356)) Then
            TempSettings.Negotiation = Settings.DefaultNegotiation
        Else
            TempSettings.Negotiation = CInt(cmbDCNegotiation.Text)
        End If

        If cmbDCResearchMgmt.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(12179)) Then
            TempSettings.ResearchProjectMgt = Settings.DefaultResearchProjMgt
        Else
            TempSettings.ResearchProjectMgt = CInt(cmbDCResearchMgmt.Text)
        End If

        TempSettings.ColumnSort = DCColumnClicked

        If DCColumnSortType = SortOrder.Ascending Then
            TempSettings.ColumnSortType = "Ascending"
        Else
            TempSettings.ColumnSortType = "Decending"
        End If

        ' Save the data in the XML file
        Call Settings.SaveDatacoreSettings(TempSettings)

        ' Save the data to the local variable
        UserDCTabSettings = TempSettings

        MsgBox("Settings Saved", vbInformation, Application.ProductName)

    End Sub

    ' Refresh's the tab with agents
    Private Sub btnDCRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDCRefresh.Click
        Dim i As Integer
        Dim j As Integer ' for highlighting top number of agents the user can use
        Dim UniqueAgentList() As String
        Dim TotalIPH As Double = 0 ' For storing the total IPH for the top agents used
        Dim SQL As String
        Dim NegotationSkill As Integer = CInt(cmbDCNegotiation.Text)
        Dim ConnectionsSkill As Integer = CInt(cmbDCConnections.Text)
        Dim TotalRDAgents As Integer = CInt(cmbDCResearchMgmt.Text) + 1
        Dim ResearchTypeList As String = "('"
        Dim CorporationList As String = "('"
        Dim SystemSecurityCheck As String = ""

        Dim readerDC As SQLiteDataReader
        Dim readerDC2 As SQLiteDataReader
        Dim readerRecordCount As Integer

        Dim DCAgentList As New List(Of DCAgent)
        Dim TempDCAgentList As New List(Of DCAgent)
        Dim DCAgentRecord As DCAgent

        ' Price Updates
        Dim TypeIDs As New List(Of PriceItem)
        Dim TempItem As PriceItem = Nothing
        Dim DCTypeIDList As New List(Of PriceItem)
        Dim DCSystemList As New List(Of String)
        Dim DCRegionList As New List(Of String)

        Dim lstDCViewRow As ListViewItem
        Dim CanUseAgent As Boolean
        Dim CoreSkillLevel As Integer
        Dim FactionString As String = ""

        ' Standings
        Dim BaseAgentStanding As Double
        Dim BaseFactionStanding As Double
        Dim AgentStanding As Double
        Dim AgentEffectiveStanding As Double
        Dim CorpStanding As Double
        Dim FactionStanding As Double
        Dim ReqStanding As Double
        Dim ReqCorpStanding As Double

        Dim AgentLevel As Integer
        Dim Diplomacy As Integer
        Dim RPPerDay As Double
        'Dim Multiplier As Integer ' Removed with Inferno 5/22/2012
        Dim CoreSkillName As String

        ' Start
        Me.Cursor = Cursors.WaitCursor
        pnlStatus.Text = "Loading Agents..."
        Application.DoEvents()

        ' Load the Research names
        For i = 1 To DCSkillCheckBoxes.Count - 1
            If DCSkillCheckBoxes(i).Checked Then
                ' Safe this one
                ResearchTypeList = ResearchTypeList & DCSkillLabels(i).Text & "','"
            End If
        Next

        ' Format the last list
        ResearchTypeList = ResearchTypeList.Substring(0, ResearchTypeList.Length - 2) & ")"

        ' Load the Corporations
        For i = 1 To DCCorpCheckBoxes.Count - 1
            If DCCorpCheckBoxes(i).Checked Then
                CorporationList = CorporationList & DCCorpLabels(i).Text & "','"
            End If
        Next

        ' Format the last list
        CorporationList = CorporationList.Substring(0, CorporationList.Length - 2) & ")"

        ' If no corps, or skills, then exit
        If ResearchTypeList = ")" Or CorporationList = ")" Then
            MsgBox("No Datacore Agents for Selected Options", vbInformation, Application.ProductName)
            pnlStatus.Text = ""
            Me.Cursor = Cursors.Default
            Exit Sub
        End If

        ' See if they want high sec and low sec, or just high or just low sec agents - Low sec includes Null
        If chkDCHighSecAgents.Checked And Not chkDCLowSecAgents.Checked Then
            SystemSecurityCheck = " AND ROUND(SECURITY,1) >= 0.5 "
        ElseIf chkDCLowSecAgents.Checked And Not chkDCHighSecAgents.Checked Then
            SystemSecurityCheck = " AND ROUND(SECURITY,1) < 0.5 "
        End If

        ' Get count first
        SQL = "SELECT COUNT(*) FROM RESEARCH_AGENTS WHERE RESEARCH_TYPE IN " & ResearchTypeList & " AND CORPORATION_NAME IN " & CorporationList & SystemSecurityCheck

        Dim CMDCount As New SQLiteCommand(SQL, EVEDB.DBREf)
        readerRecordCount = CInt(CMDCount.ExecuteScalar())

        ' Read the settings and stats to make the query
        SQL = "SELECT FACTION, CORPORATION_ID, CORPORATION_NAME, AGENT_NAME, LEVEL, QUALITY, RESEARCH_TYPE_ID, "
        SQL = SQL & "RESEARCH_TYPE, REGION_ID, REGION_NAME, SOLAR_SYSTEM_ID, SOLAR_SYSTEM_NAME, SECURITY, STATION "
        SQL = SQL & "FROM RESEARCH_AGENTS, FACTIONS, REGIONS "
        SQL = SQL & "WHERE RESEARCH_AGENTS.REGION_ID = REGIONS.regionID "
        SQL = SQL & "AND REGIONS.factionID = FACTIONS.factionID "
        SQL = SQL & "AND RESEARCH_TYPE IN " & ResearchTypeList & " AND CORPORATION_NAME IN " & CorporationList & SystemSecurityCheck

        FactionString = "AND FACTIONS.factionName in ("

        ' Set Sov check
        If chkDCAmarrSov.Checked Then
            FactionString = FactionString & "'Amarr Empire',"
        End If
        If chkDCAmmatarSov.Checked Then
            FactionString = FactionString & "'Ammatar Mandate',"
        End If
        If chkDCCaldariSov.Checked Then
            FactionString = FactionString & "'Caldari State',"
        End If
        If chkDCGallenteSov.Checked Then
            FactionString = FactionString & "'Gallente Federation',"
        End If
        If chkDCKhanidSov.Checked Then
            FactionString = FactionString & "'Khanid Kingdom',"
        End If
        If chkDCMinmatarSov.Checked Then
            FactionString = FactionString & "'Minmatar Republic',"
        End If
        If chkDCSyndicateSov.Checked Then
            FactionString = FactionString & "'The Syndicate',"
        End If
        If chkDCThukkerSov.Checked Then
            FactionString = FactionString & "'Thukker Tribe',"
        End If

        If FactionString <> "AND FACTIONS.factionName in (" Then
            FactionString = FactionString.Substring(0, Len(FactionString) - 1) & ") "
            ' Add the faction string
            SQL = SQL & FactionString
        Else
            ' Clear
            MsgBox("No Datacore Agents for Selected Options", vbInformation, Application.ProductName)
            lstDC.Items.Clear()
            GoTo Leave
        End If

        If cmbDCRegions.Text <> "All Regions" Then
            SQL = SQL & " AND regionName = '" & cmbDCRegions.Text & "'"
        End If

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerDC = DBCommand.ExecuteReader

        If Not readerDC.HasRows Then
            ' Clear and exit
            MsgBox("No Datacore Agents for Selected Options", vbInformation, Application.ProductName)
            lstDC.Items.Clear()
            GoTo Leave
        End If

        pnlProgressBar.Value = 0
        pnlProgressBar.Visible = True
        pnlProgressBar.Maximum = readerRecordCount

        ' Loop through all the agents and build the list to put into the graph
        While readerDC.Read
            ' First find out what personal standing to use, faction, corp or personal
            ' Get user's base agent standing with this agent
            If Not IsNothing(SelectedCharacter.Standings) Then
                BaseAgentStanding = SelectedCharacter.Standings.GetStanding(readerDC.GetString(3))
                BaseFactionStanding = SelectedCharacter.Standings.GetStanding(readerDC.GetString(0))
            Else
                BaseAgentStanding = 0
                BaseFactionStanding = 0
            End If

            ' The corp standing will either be the original corp standing or whatever they put in (skills are applied in text box display)
            CorpStanding = GetDCtxtCorpStanding(readerDC.GetString(2))

            Diplomacy = SelectedCharacter.Skills.GetSkillLevel(3357)

            ' Standing = (BaseStanding + (10 - BaseStanding) * (0.04 * ConnectionsSkill))
            If BaseAgentStanding < 0 Then
                ' Use Diplomacy
                AgentStanding = BaseAgentStanding + ((10 - BaseAgentStanding) * (0.04 * Diplomacy))
            ElseIf BaseAgentStanding > 0 Then
                ' Use connections
                AgentStanding = BaseAgentStanding + ((10 - BaseAgentStanding) * (0.04 * ConnectionsSkill))
            Else
                AgentStanding = 0
            End If

            If BaseFactionStanding < 0 Then
                ' Use Diplomacy
                FactionStanding = BaseFactionStanding + ((10 - BaseFactionStanding) * (0.04 * Diplomacy))
            ElseIf BaseFactionStanding > 0 Then
                ' Use connections
                FactionStanding = BaseFactionStanding + ((10 - BaseFactionStanding) * (0.04 * ConnectionsSkill))
            Else
                FactionStanding = 0
            End If

            '******* Agent Quality Removed on 19 May 2011 *******
            ' Base Quality is now 20 for all agents
            AgentLevel = readerDC.GetInt32(4)

            ' Agent_Effective_Quality = Agent_Quality + (5 * Negotiation_Skill_Level) + Round_Down(AgentPersonalStanding)
            AgentEffectiveStanding = 20 + (5 * NegotationSkill) + AgentStanding

            ' Required Standing = ((Level - 1) * 2) + (Quality / 20) 
            ' ReqStanding = (AgentLevel - 1) * 2 -- May 19th change
            Select Case AgentLevel
                Case 1
                    ReqStanding = 0
                Case 2
                    ReqStanding = 1
                Case 3
                    ReqStanding = 3
                Case 4
                    ReqStanding = 5
            End Select

            ReqCorpStanding = ReqStanding - 2

            ' Now determine if we can use this agent or not based on all the data
            ' New from game: Your effective personal standings must be 3.00 or higher toward this agent's corporation in order to use this agent, 
            ' as well as an effective personal standing of 5.00 or higher toward this agent, its faction, or its corporation in order to use this agent's services.

            ' From game: Your effective personal standings must be 2.00 or higher toward this agent's 
            ' corporation in order to use this agent (level 3, qual 0, effqual 20), 
            ' as well as an effective personal standing of 4.00 or higher toward this agent, its faction, 
            ' or its corporation in order to use this agent's services.

            If CorpStanding >= ReqCorpStanding And (AgentStanding >= ReqStanding Or CorpStanding >= ReqStanding Or FactionStanding >= ReqStanding) Then
                ' Can use agent
                CanUseAgent = True
            Else
                ' Can't so grey out line
                CanUseAgent = False
            End If

            CoreSkillName = readerDC.GetString(7)
            CoreSkillLevel = GetCoreSkillLevel(CoreSkillName)

            ' Research_Points_Per_Day = Multiplier * ((1 + (Agent_Effective_Quality / 100)) *  ((Your_Skill + Agent_Skill) ^ 2))
            RPPerDay = Math.Round(((CoreSkillLevel + AgentLevel) ^ 2) * (1 + (AgentEffectiveStanding / 100)), 2)

            ' Now load information into list
            DCAgentRecord.SystemID = readerDC.GetInt64(10)
            DCAgentRecord.SystemSecurity = Math.Round(readerDC.GetDouble(12), 1)
            DCAgentRecord.RegionID = readerDC.GetInt64(8)
            DCAgentRecord.Faction = readerDC.GetString(0)
            DCAgentRecord.FactionStanding = FactionStanding
            DCAgentRecord.Corporation = readerDC.GetString(2)
            DCAgentRecord.CorporationStanding = CorpStanding
            DCAgentRecord.Agent = readerDC.GetString(3)
            DCAgentRecord.AgentStanding = Math.Truncate(AgentStanding * 100) / 100
            DCAgentRecord.AgentLevel = readerDC.GetInt32(4)
            DCAgentRecord.AgentLocation = readerDC.GetString(13) & " (" & CStr(DCAgentRecord.SystemSecurity) & ") - " & readerDC.GetString(9)  ' Station name + security + region

            ' Need the Core typeID
            Dim TempCoreName As String = ""

            If readerDC.GetString(7).Contains("Amarr Starship") Then
                TempCoreName = "Amarrian Starship Engineering"
            ElseIf readerDC.GetString(7).Contains("Gallente Starship") Then
                TempCoreName = "Gallentean Starship Engineering"
            Else
                TempCoreName = readerDC.GetString(7)
            End If

            SQL = "SELECT typeID FROM INVENTORY_TYPES WHERE typeName = 'Datacore - " & TempCoreName & "'"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerDC2 = DBCommand.ExecuteReader()
            If readerDC2.Read() Then
                DCAgentRecord.DataCoreID = readerDC2.GetInt64(0)
            Else
                DCAgentRecord.DataCoreID = 0
            End If
            readerDC2.Close()
            readerDC2 = Nothing

            DCAgentRecord.DataCoreSkill = readerDC.GetString(7)
            DCAgentRecord.DataCoreSkillLevel = CoreSkillLevel
            DCAgentRecord.AgentAvailable = CanUseAgent
            DCAgentRecord.RPperDay = RPPerDay
            DCAgentRecord.CoresPerDay = RPPerDay / 100 ' Inferno Change - all cores cost 100 RP per core
            DCAgentRecord.DataCorePrice = 0
            DCAgentRecord.PriceFrom = ""
            DCAgentRecord.IskPerHour = 0

            ' Add the record
            DCAgentList.Add(DCAgentRecord)

            ' For each record, update the progress bar
            Call IncrementToolStripProgressBar(pnlProgressBar)

        End While

        readerDC.Close()
        readerDC = Nothing
        DBCommand = Nothing

        pnlProgressBar.Visible = False
        Application.DoEvents()

        pnlStatus.Text = "Calculating..."
        pnlProgressBar.Value = 0
        pnlProgressBar.Maximum = DCAgentList.Count
        pnlProgressBar.Visible = True
        Application.DoEvents()

        ' Now figure out what prices to use and load accordingly
        If rbtnDCUpdatedPrices.Checked Then ' Use whatever the user has loaded in Item Prices
            For i = 0 To DCAgentList.Count - 1
                ' First get the record we are updating
                SQL = "SELECT PRICE FROM ITEM_PRICES WHERE ITEM_ID =" & DCAgentList(i).DataCoreID

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerDC2 = DBCommand.ExecuteReader()

                ' First save the record and then remove the record we are on
                DCAgentRecord = DCAgentList(i)

                If readerDC2.Read() Then
                    DCAgentRecord.DataCorePrice = readerDC2.GetDouble(0)
                Else
                    DCAgentRecord.DataCorePrice = 0
                End If

                DCAgentRecord.DataCorePrice -= DataCoreRedeemCost ' Add an amount of Isk to redeem each datacore, so subtract this from the market price

                DCAgentRecord.PriceFrom = "Current"
                DCAgentRecord.IskPerHour = Math.Round((DCAgentRecord.DataCorePrice * DCAgentRecord.CoresPerDay) / 24, 2)

                ' Insert the record
                TempDCAgentList.Add(DCAgentRecord)

                readerDC2.Close()
                readerDC2 = Nothing
                DBCommand = Nothing
            Next

        ElseIf rbtnDCRegionPrices.Checked Then ' Look up the max buy price for the region the Agent is located
            ' Update the price cache with our list of Regions for these datacores
            ' Build the list of datacore ID's and Regions
            For i = 0 To DCAgentList.Count - 1
                TempItem.Manufacture = False
                TempItem.TypeID = DCAgentList(i).DataCoreID
                TempItem.GroupName = GetPriceGroupName(TempItem.TypeID)
                TempItem.PriceModifier = 0
                TempItem.SystemID = ""

                Dim TempRegionList As New List(Of String)
                TempItem.RegionID = DCRegionList(j)

                If Not DCTypeIDList.Contains(TempItem) Then
                    DCTypeIDList.Add(TempItem)
                End If

                If Not DCRegionList.Contains(CStr(DCAgentList(i).RegionID)) Then
                    DCRegionList.Add(CStr(DCAgentList(i).RegionID))
                End If

            Next

            ' Update
            Call UpdatePricesCache(DCTypeIDList)

            ' Now search for each item's price in the cache with its region and pull up the max buy order
            For i = 0 To DCAgentList.Count - 1
                SQL = "SELECT buyMax FROM ITEM_PRICES_CACHE WHERE typeID =" & DCAgentList(i).DataCoreID & " AND RegionOrSystem ='" & DCAgentList(i).RegionID & "'"

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerDC2 = DBCommand.ExecuteReader()

                DCAgentRecord = DCAgentList(i)

                If readerDC2.Read Then
                    DCAgentRecord.DataCorePrice = readerDC2.GetDouble(0)
                Else
                    DCAgentRecord.DataCorePrice = 0
                End If

                DCAgentRecord.DataCorePrice -= DataCoreRedeemCost ' Add an amount of Isk to redeem each datacore, so subtract this from the market price

                DCAgentRecord.PriceFrom = "Region"
                DCAgentRecord.IskPerHour = Math.Round((DCAgentRecord.DataCorePrice * DCAgentRecord.CoresPerDay) / 24, 2)

                ' Insert the record
                TempDCAgentList.Add(DCAgentRecord)

                readerDC2.Close()
                readerDC2 = Nothing
                DBCommand = Nothing
            Next

        ElseIf rbtnDCSystemPrices.Checked Then ' Use the max buy price for the system the Agent is located
            ' Update the price cache with a list of systems for these datacores
            ' Build the list of datacore ID's and systems
            For i = 0 To DCAgentList.Count - 1
                TempItem.Manufacture = False
                TempItem.TypeID = DCAgentList(i).DataCoreID
                TempItem.GroupName = GetPriceGroupName(TempItem.TypeID)
                TempItem.PriceModifier = 0
                TempItem.SystemID = CStr(DCAgentList(i).SystemID)
                TempItem.RegionID = ""

                If Not DCTypeIDList.Contains(TempItem) Then
                    DCTypeIDList.Add(TempItem)
                End If

                If Not DCSystemList.Contains(CStr(DCAgentList(i).SystemID)) Then
                    DCSystemList.Add(CStr(DCAgentList(i).SystemID))
                End If

            Next

            ' Need to update the cache for each region, for all typeids so send one system at a time
            For i = 0 To DCSystemList.Count - 1
                Call UpdatePricesCache(DCTypeIDList)
            Next

            ' Now search for each item's price in the cache with its solar system and pull up the max buy order
            For i = 0 To DCAgentList.Count - 1
                SQL = "SELECT buyMax FROM ITEM_PRICES_CACHE WHERE typeID =" & DCAgentList(i).DataCoreID & " AND RegionOrSystem ='" & DCAgentList(i).SystemID & "'"

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerDC2 = DBCommand.ExecuteReader()

                DCAgentRecord = DCAgentList(i)

                If readerDC2.Read Then
                    DCAgentRecord.DataCorePrice = readerDC2.GetDouble(0)
                Else
                    DCAgentRecord.DataCorePrice = 0
                End If

                DCAgentRecord.DataCorePrice -= DataCoreRedeemCost ' Add an amount of Isk to redeem each datacore, so subtract this from the market price

                DCAgentRecord.PriceFrom = "System"
                DCAgentRecord.IskPerHour = Math.Round((DCAgentRecord.DataCorePrice * DCAgentRecord.CoresPerDay) / 24, 2)

                ' Insert the record
                TempDCAgentList.Add(DCAgentRecord)

                readerDC2.Close()
                readerDC2 = Nothing
                DBCommand = Nothing
            Next

        End If

        ' Set the updated list
        DCAgentList = TempDCAgentList
        ' Sort by IPH for calculating the top 5
        DCAgentList.Sort(New DataCoreIPHComparer)

        ' Get number of R&D Agents they can use
        ReDim UniqueAgentList(TotalRDAgents - 1)
        j = 0

        pnlStatus.Text = "Refreshing List..."
        pnlProgressBar.Value = 0
        pnlProgressBar.Maximum = DCAgentList.Count
        pnlProgressBar.Visible = True
        Application.DoEvents()

        ' Build a list of unique agent names equal to number the user can use in order of Isk per hour
        For i = 0 To DCAgentList.Count - 1
            If DCAgentList(i).AgentAvailable Or (Not DCAgentList(i).AgentAvailable And chkDCIncludeAllAgents.Checked) Then ' make sure we want to look at this one
                If Not UniqueAgentList.Contains(DCAgentList(i).Agent) Then
                    ' Add the agent
                    UniqueAgentList(j) = DCAgentList(i).Agent
                    j = j + 1

                    If j > TotalRDAgents - 1 Then
                        Exit For
                    End If
                End If
            End If
        Next

        j = 0
        lstDC.Items.Clear()

        ' Load the data into the table
        lstDC.BeginUpdate()
        For i = 0 To DCAgentList.Count - 1
            If DCAgentList(i).AgentAvailable Or (Not DCAgentList(i).AgentAvailable And chkDCIncludeAllAgents.Checked) Then
                lstDCViewRow = New ListViewItem("") ' Check
                'The remaining columns are subitems  
                lstDCViewRow.SubItems.Add(DCAgentList(i).Corporation)
                lstDCViewRow.SubItems.Add(DCAgentList(i).Agent)
                lstDCViewRow.SubItems.Add(CStr(DCAgentList(i).AgentLevel))
                lstDCViewRow.SubItems.Add(FormatNumber(DCAgentList(i).AgentStanding, 2))
                lstDCViewRow.SubItems.Add(DCAgentList(i).AgentLocation)
                lstDCViewRow.SubItems.Add(DCAgentList(i).DataCoreSkill)
                lstDCViewRow.SubItems.Add(FormatNumber(DCAgentList(i).DataCorePrice, 2))
                lstDCViewRow.SubItems.Add(DCAgentList(i).PriceFrom)
                lstDCViewRow.SubItems.Add(FormatNumber(DCAgentList(i).CoresPerDay, 2))
                lstDCViewRow.SubItems.Add(FormatNumber(DCAgentList(i).IskPerHour, 2))

                ' Color in the top 5 unique agents
                If j <= TotalRDAgents - 1 Then
                    If DCAgentList(i).Agent = UniqueAgentList(j) Then
                        ' Color this row
                        lstDCViewRow.BackColor = Color.LightGreen
                        ' Save the total iph for this agent and add it to the total
                        TotalIPH = TotalIPH + DCAgentList(i).IskPerHour
                        ' Move to next agent name
                        j = j + 1
                    End If
                End If

                ' If the agent can't be used, grey out the row
                If Not DCAgentList(i).AgentAvailable Then
                    lstDCViewRow.ForeColor = Color.Gray
                Else
                    Select Case DCAgentList(i).SystemSecurity
                        Case Is < 0.1
                            lstDCViewRow.ForeColor = Color.Red
                        Case Is < 0.5
                            lstDCViewRow.ForeColor = Color.Orange
                        Case Else
                            lstDCViewRow.ForeColor = Color.Black
                    End Select
                End If

                ' Finally, highlight with blue text the agents we have selected
                For Each DBAgent In SelectedCharacter.GetResearchAgents.GetResearchAgents
                    If DCAgentList(i).Agent = DBAgent.Agent And DCAgentList(i).AgentLevel = DBAgent.AgentLevel _
                        And DCAgentList(i).DataCoreSkill = DBAgent.Field And DCAgentList(i).AgentAvailable Then
                        lstDCViewRow.ForeColor = Color.Blue
                        Exit For
                    End If
                Next

                Call lstDC.Items.Add(lstDCViewRow)

            End If

            ' For each record, update the progress bar
            Call IncrementToolStripProgressBar(pnlProgressBar)

        Next

        ' Now sort this
        Dim TempType As SortOrder
        If DCColumnSortType = SortOrder.Ascending Then
            TempType = SortOrder.Descending
        Else
            TempType = SortOrder.Ascending
        End If
        Call ListViewColumnSorter(DCColumnClicked, CType(lstDC, ListView), DCColumnClicked, TempType)
        Me.Cursor = Cursors.Default
        lstDC.EndUpdate()

        ' Update the Total IPH
        txtDCTotalOptIPH.Text = FormatNumber(TotalIPH, 2)

Leave:
        ' End
        Me.Cursor = Cursors.Default
        pnlStatus.Text = ""
        pnlProgressBar.Visible = False
        Application.DoEvents()

    End Sub

    ' Returns the text standing in the DC box for the corp name sent
    Private Function GetDCtxtCorpStanding(ByVal CorpName As String) As Double
        ' Load the Research names
        For i = 1 To DCCorpLabels.Count - 1
            ' Compare to the label (all indexes are synched)
            If DCCorpLabels(i).Text = CorpName Then
                ' Return the value of the text box
                If Trim(DCCorpTextboxes(i).Text) = "" Or Not IsNumeric(DCCorpTextboxes(i).Text) Then
                    DCCorpTextboxes(i).Text = "0.00"
                    Return 0
                Else
                    Return CDbl(DCCorpTextboxes(i).Text)
                End If
            End If
        Next

        Return 0
    End Function

    ' Returns the skill set in the combo boxes that is sent
    Private Function GetCoreSkillLevel(ByVal SkillName As String) As Integer
        Dim i As Integer

        For i = 1 To DCSkillLabels.Count - 1
            If DCSkillLabels(i).Text = SkillName Then
                Return CInt(DCSkillCombos(i).Text)
            End If
        Next

        Return 0

    End Function

    ' Loads the corporation standings on the Datacore screen
    Private Sub LoadDCCorpStandings(Optional ByVal UpdateCheckBoxes As Boolean = True)
        Dim Settings As New ProgramSettings

        ' Load the Corporation Standings with skills
        If SelectedCharacter.Name <> None Then
            For i = 1 To DCCorpLabels.Count - 1

                ' Check based on default
                If UserDCTabSettings.CorpsStanding(i - 1) = Settings.DefaultCorpStanding Then
                    DCCorpTextboxes(i).Text = FormatNumber(SelectedCharacter.Standings.GetEffectiveStanding(DCCorpLabels(i).Text, CInt(cmbDCConnections.Text), SelectedCharacter.Skills.GetSkillLevel(3357)), 2)
                Else ' use what they entered
                    DCCorpTextboxes(i).Text = FormatNumber(UserDCTabSettings.CorpsStanding(i - 1), 2)
                End If

                If UpdateCheckBoxes Then
                    ' check it based on defaults
                    If UserDCTabSettings.CorpsChecked(i - 1) = Settings.DefaultCorpStandingChecked Then
                        If DCCorpTextboxes(i).Text <> "0.00" Then
                            DCCorpCheckBoxes(i).Checked = True
                            DCCorpTextboxes(i).Enabled = True
                        Else
                            DCCorpCheckBoxes(i).Checked = False
                            DCCorpTextboxes(i).Enabled = False
                        End If
                    Else ' What they set
                        DCCorpCheckBoxes(i).Checked = CBool(UserDCTabSettings.CorpsChecked(i - 1))
                        DCCorpTextboxes(i).Enabled = CBool(UserDCTabSettings.CorpsChecked(i - 1))
                    End If

                End If

            Next

            If UpdateCheckBoxes Then
                txtDCTotalOptIPH.Text = "0.00"
                txtDCTotalSelectedIPH.Text = "0.00"
                lstDC.Items.Clear()
            End If
        Else
            ' Using dummy, so uncheck all and disable all boxes
            For i = 1 To DCCorpLabels.Count - 1
                DCCorpCheckBoxes(i).Checked = False
                DCCorpTextboxes(i).Enabled = False
                DCCorpTextboxes(i).Text = "0.00"
            Next
        End If

    End Sub

    Private Sub btnDCExporttoClip_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDCExporttoClip.Click
        Dim ClipboardData = New DataObject
        Dim OutputText As String = ""
        Dim item As ListViewItem
        Dim checkeditems As ListView.CheckedListViewItemCollection
        Dim ItemsSelected As Boolean = False
        Dim Separator As String = ""

        If lstDC.CheckedItems.Count <> 0 Then
            ' For each checked item, export the data
            checkeditems = lstDC.CheckedItems

            If UserApplicationSettings.DataExportFormat = CSVDataExport Then
                Separator = ", "
            ElseIf UserApplicationSettings.DataExportFormat = SSVDataExport Then
                Separator = "; "
            End If

            If UserApplicationSettings.DataExportFormat = CSVDataExport Then
                OutputText = "Selected R&D Agents:" & Environment.NewLine & "Corporation, Agent Name, Agent Level, Agent Location, DataCore Skill, Isk per Hour" & Environment.NewLine & Environment.NewLine
            ElseIf UserApplicationSettings.DataExportFormat = SSVDataExport Then
                OutputText = "Selected R&D Agents:" & Environment.NewLine & "Corporation; Agent Name; Agent Level; Agent Location; DataCore Skill; Isk per Hour" & Environment.NewLine & Environment.NewLine
            Else
                OutputText = "Selected R&D Agents:" & Environment.NewLine & Environment.NewLine
            End If

            For Each item In checkeditems
                ItemsSelected = True
                If UserApplicationSettings.DataExportFormat = CSVDataExport Or UserApplicationSettings.DataExportFormat = SSVDataExport Then
                    OutputText = OutputText & item.SubItems(1).Text & Separator & item.SubItems(2).Text & Separator & item.SubItems(3).Text & Separator
                    OutputText = OutputText & item.SubItems(5).Text & Separator & item.SubItems(6).Text & Separator
                    If UserApplicationSettings.DataExportFormat = SSVDataExport Then
                        ' Replace any commas with decimals and vice versa
                        OutputText = OutputText & ConvertUStoEUDecimal(item.SubItems(10).Text) & Separator & Environment.NewLine
                    Else ' Save as normal but remove any commas in the price
                        OutputText = OutputText & item.SubItems(10).Text.Replace(",", "") & Separator & Environment.NewLine
                    End If
                Else
                    OutputText = OutputText & "Corporation: " & item.SubItems(1).Text & Environment.NewLine
                    OutputText = OutputText & "Agent Name: " & item.SubItems(2).Text & Environment.NewLine
                    OutputText = OutputText & "Agent Level: " & item.SubItems(3).Text & Environment.NewLine
                    OutputText = OutputText & "Agent Location: " & item.SubItems(5).Text & Environment.NewLine
                    OutputText = OutputText & "Datacore Skill: " & item.SubItems(6).Text & Environment.NewLine
                    OutputText = OutputText & "Isk per Hour: " & item.SubItems(10).Text & Environment.NewLine & Environment.NewLine
                End If
            Next

            If ItemsSelected Then
                ' Paste to clipboard
                Call CopyTextToClipboard(OutputText)
            End If
        Else
            MsgBox("No Agents Selected", vbInformation, Application.ProductName)
        End If

    End Sub

    Private Sub cmbDCRegions_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbDCRegions.DropDown
        Dim SQL As String
        Dim readerReg As SQLiteDataReader

        If Not DCRegionsLoaded Then

            ' Load the select systems combobox with systems
            SQL = "SELECT regionName FROM RESEARCH_AGENTS, REGIONS WHERE RESEARCH_AGENTS.REGION_ID = REGIONS.regionID "
            SQL = SQL & "GROUP BY regionName"

            DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
            readerReg = DBCommand.ExecuteReader

            cmbDCRegions.Items.Add("All Regions")

            While readerReg.Read
                cmbDCRegions.Items.Add(readerReg.GetString(0))
            End While

            readerReg.Close()
            readerReg = Nothing
            DBCommand = Nothing

            cmbDCRegions.Text = "All Regions"
            DCRegionsLoaded = True

        End If

    End Sub

    ' For sorting a list of Mining Ore
    Public Class DataCoreIPHComparer

        Implements System.Collections.Generic.IComparer(Of DCAgent)

        Public Function Compare(ByVal p1 As DCAgent, ByVal p2 As DCAgent) As Integer Implements IComparer(Of DCAgent).Compare
            ' swap p2 and p1 to do decending sort
            Return p2.IskPerHour.CompareTo(p1.IskPerHour)
        End Function

    End Class

#End Region

#Region "Reactions"

#Region "Reaction Form functions"

    ' Calculate per hour, the best profit available for reactions selected
    Private Sub btnCalculateCosts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReactionRefresh.Click
        lstReactionMats.Visible = False
        Call UpdateReactionsGrid()

    End Sub

    Private Sub chkReactionsTaxes_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsTaxes.CheckedChanged
        If Not FirstLoad Then

            ' Reset check box
            lblReactionsTaxes.Text = "0.00"

            Call UpdateReactionsGrid()
        End If
    End Sub

    Private Sub chkReactionsFees_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsFees.CheckedChanged
        If Not FirstLoad Then

            ' Reset check box
            lblReactionsFees.Text = "0.00"

            Call UpdateReactionsGrid()
        End If
    End Sub

    Private Sub txtReactionsNumPOS_GotFocus(sender As Object, e As System.EventArgs) Handles txtReactionsNumPOS.GotFocus
        Call txtReactionsNumPOS.SelectAll()
    End Sub

    Private Sub txtReactionsNumPOS_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtReactionsNumPOS.KeyDown
        Call ProcessCutCopyPasteSelect(txtReactionsNumPOS, e)
        If e.KeyCode = Keys.Enter Then
            Call UpdateReactionsGrid()
        End If
    End Sub

    Private Sub txtReactionsNumPOS_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtReactionsNumPOS.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtReactionPOSFuelCost_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtReactionPOSFuelCost.Click
        Call txtReactionPOSFuelCost.SelectAll()
    End Sub

    Private Sub txtReactionPOSFuelCost_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtReactionPOSFuelCost.GotFocus
        Call txtReactionPOSFuelCost.SelectAll()
    End Sub

    Private Sub txtReactionPOSFuelCost_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtReactionPOSFuelCost.KeyDown
        Call ProcessCutCopyPasteSelect(txtReactionPOSFuelCost, e)
        If e.KeyCode = Keys.Enter Then
            Call UpdateReactionsGrid()
        End If
    End Sub

    Private Sub txtReactionPOSFuelCost_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtReactionPOSFuelCost.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub chkReactionsMoonMats_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsProcMoonMats.CheckedChanged
        Call UpdateReactionChecks()
    End Sub

    Private Sub chkReactionsAdvComp_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsAdvMoonMats.CheckedChanged
        Call UpdateReactionChecks()
    End Sub

    Private Sub chkReactionsSimpleBio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsSimpleBio.CheckedChanged
        Call UpdateReactionChecks()
    End Sub

    Private Sub chkReactionsComplexBio_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsComplexBio.CheckedChanged
        Call UpdateReactionChecks()
    End Sub

    Private Sub chkReactionsBuildBasic_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkReactionsBuildBasic.CheckedChanged
        If chkReactionsBuildBasic.Checked Then
            chkReactionsIgnoreBaseMatPrice.Enabled = True
        Else
            chkReactionsIgnoreBaseMatPrice.Enabled = False
        End If
    End Sub

    Private Sub chkReactionsBuildBasic_EnabledChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkReactionsBuildBasic.EnabledChanged
        If Not chkReactionsBuildBasic.Enabled Then
            chkReactionsIgnoreBaseMatPrice.Enabled = False
        End If
    End Sub

    Private Sub UpdateReactionChecks()
        If chkReactionsAdvMoonMats.Checked Or chkReactionsComplexBio.Checked Then
            chkReactionsBuildBasic.Enabled = True
        Else
            chkReactionsBuildBasic.Enabled = False
        End If

        If chkReactionsSimpleBio.Checked Or chkReactionsProcMoonMats.Checked Then
            chkReactionsIgnoreBaseMatPrice.Enabled = True
        Else
            chkReactionsIgnoreBaseMatPrice.Enabled = False
        End If

        If chkReactionsProcMoonMats.Checked Then
            chkReactionsRefine.Enabled = True
        Else
            chkReactionsRefine.Enabled = False
        End If

    End Sub

    Private Sub lstReactions_ColumnClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstReactions.ColumnClick
        Call ListViewColumnSorter(e.Column, lstReactions, ReactionsColumnClicked, ReactionsColumnSortType)
    End Sub

    Private Sub chkReactionsRefine_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkReactionsRefine.CheckedChanged
        If chkReactionsRefine.Checked Then
            gbReactionsRefinery.Enabled = True
        Else
            gbReactionsRefinery.Enabled = False
        End If
    End Sub

#End Region

    Private Sub InitReactionsTab()
        lstReactions.Items.Clear()
        lstReactionMats.Visible = False

        chkReactionsTaxes.Checked = UserReactionTabSettings.CheckTaxes
        chkReactionsFees.Checked = UserReactionTabSettings.CheckFees

        ReactionsColumnClicked = UserReactionTabSettings.ColumnSort
        If UserReactionTabSettings.ColumnSortType = "Ascending" Then
            ReactionsColumnSortType = SortOrder.Ascending
        Else
            ReactionsColumnSortType = SortOrder.Descending
        End If

        chkReactionsAdvMoonMats.Checked = UserReactionTabSettings.CheckAdvMoonMats
        chkReactionsProcMoonMats.Checked = UserReactionTabSettings.CheckProcessedMoonMats
        chkReactionsHybrid.Checked = UserReactionTabSettings.CheckHybrid

        chkReactionsSimpleBio.Checked = UserReactionTabSettings.CheckSimpleBio
        chkReactionsComplexBio.Checked = UserReactionTabSettings.CheckComplexBio

        chkReactionsRefine.Checked = UserReactionTabSettings.CheckRefine
        chkReactionsIgnoreBaseMatPrice.Checked = UserReactionTabSettings.CheckIgnoreMarket
        chkReactionsBuildBasic.Checked = UserReactionTabSettings.CheckBuildBasic

        cmbReactionsRefineTax.Text = FormatPercent(UserReactionTabSettings.RefineryTax, 1)
        cmbReactionsRefiningEfficiency.Text = FormatPercent(UserReactionTabSettings.RefineryEfficiency, 0)
        txtReactionsRefineryStanding.Text = FormatNumber(UserReactionTabSettings.RefineryStanding, 2)

        txtReactionsNumPOS.Text = CStr(UserReactionTabSettings.NumberofPOS)
        txtReactionPOSFuelCost.Text = FormatNumber(UserReactionTabSettings.POSFuelCost, 2)
        txtReactionPOSFuelCost.Focus()

    End Sub

    Private Sub btnReactionsSaveSettings_Click(sender As System.Object, e As System.EventArgs) Handles btnReactionsSaveSettings.Click
        Dim TempSettings As ReactionsTabSettings = Nothing
        Dim Settings As New ProgramSettings

        If Not IsNumeric(txtReactionPOSFuelCost.Text) Or Trim(txtReactionPOSFuelCost.Text) = "" Then
            MsgBox("Invalid POS Fuel Cost", vbExclamation, Application.ProductName)
            txtReactionPOSFuelCost.Focus()
            Exit Sub
        End If

        If Not IsNumeric(txtReactionPOSFuelCost.Text) Or Trim(txtReactionsNumPOS.Text) = "" Then
            MsgBox("Invalid Number of POSs", vbExclamation, Me.Text)
            txtReactionsNumPOS.Focus()
            Exit Sub
        End If

        If Not IsNumeric(txtReactionsRefineryStanding.Text) Or Trim(txtReactionsRefineryStanding.Text) = "" Then
            MsgBox("Invalid Number of POSs", vbExclamation, Me.Text)
            txtReactionsRefineryStanding.Focus()
            Exit Sub
        End If

        If Val(txtReactionsRefineryStanding.Text) > 10 Then
            MsgBox("Choose a lower value for Refinery Standing", vbExclamation, Me.Text)
            txtReactionsRefineryStanding.Focus()
            Exit Sub
        End If

        ' Refine Tax
        Dim TempRefine As String
        TempRefine = cmbReactionsRefineTax.Text.Replace("%", "")

        If Not IsNumeric(TempRefine) Or Trim(TempRefine) = "" Then
            MsgBox("Invalid Refinery Tax", vbExclamation, Application.ProductName)
            cmbReactionsRefineTax.Focus()
        ElseIf CDbl(TempRefine) > 10 Then
            cmbReactionsRefineTax.Text = "10.0"
        End If

        TempSettings.POSFuelCost = CDbl(txtReactionPOSFuelCost.Text)
        TempSettings.NumberofPOS = CInt(txtReactionsNumPOS.Text)

        TempSettings.CheckTaxes = chkReactionsTaxes.Checked
        TempSettings.CheckFees = chkReactionsFees.Checked
        TempSettings.CheckAdvMoonMats = chkReactionsAdvMoonMats.Checked
        TempSettings.CheckProcessedMoonMats = chkReactionsProcMoonMats.Checked
        TempSettings.CheckHybrid = chkReactionsHybrid.Checked

        TempSettings.CheckRefine = chkReactionsRefine.Checked
        TempSettings.CheckIgnoreMarket = chkReactionsIgnoreBaseMatPrice.Checked
        TempSettings.CheckBuildBasic = chkReactionsBuildBasic.Checked
        TempSettings.CheckSimpleBio = chkReactionsSimpleBio.Checked
        TempSettings.CheckComplexBio = chkReactionsComplexBio.Checked

        TempSettings.ColumnSort = ReactionsColumnClicked

        If ReactionsColumnSortType = SortOrder.Ascending Then
            TempSettings.ColumnSortType = "Ascending"
        Else
            TempSettings.ColumnSortType = "Decending"
        End If

        If cmbReactionsRefiningEfficiency.Text.Contains("%") Then
            TempSettings.RefineryEfficiency = CDbl(cmbReactionsRefiningEfficiency.Text.Substring(0, Len(cmbReactionsRefiningEfficiency.Text) - 1)) / 100
        Else
            TempSettings.RefineryEfficiency = CDbl(cmbReactionsRefiningEfficiency.Text) / 100
        End If

        ' Standings
        TempSettings.RefineryStanding = CDbl(txtReactionsRefineryStanding.Text)

        Dim RefineTax As Double = CDbl(cmbReactionsRefineTax.Text.Replace("%", ""))

        If RefineTax <= 0 Then
            TempSettings.RefineryTax = 0
        Else
            TempSettings.RefineryTax = RefineTax / 100
        End If

        ' Save the data in the XML file
        Call Settings.SaveReactionSettings(TempSettings)

        ' Save the data to the local variable
        UserReactionTabSettings = TempSettings

        MsgBox("Settings Saved", vbInformation, Application.ProductName)

    End Sub

    ' Rebuild of reaction processing
    Private Function BuildReactionList(ByVal LoadAdvMoon As Boolean, ByVal LoadSimpleMoon As Boolean, ByVal LoadHybrid As Boolean,
                                   ByVal LoadSimpleBio As Boolean, ByVal LoadAdvBio As Boolean, ByVal IgnoreBaseMatPrice As Boolean,
                                   ByVal BuildBaseMats As Boolean, ByVal SetTaxes As Boolean, ByVal SetFees As Boolean,
                                   ByVal Refine As Boolean, TotalHourlyPOSCost As Double, NumberofTowers As Integer) As List(Of Reaction)

        Dim FinalReactionList As New List(Of Reaction)
        Dim CurrentReaction As Reaction
        Dim readerReactions As SQLiteDataReader
        Dim SQL As String
        Dim ReactionGroupList As String = ""

        Dim ReprocessingStation As RefiningReprocessing
        Dim RefineOutputName As String
        Dim RefineOutputQuantity As Long
        Dim RefineOutputVolume As Double
        Dim TempMats As Materials

        ' Get the list of reactions we want to make
        SQL = "SELECT REACTION_TYPE_ID, REACTION_NAME, REACTION_GROUP, MATERIAL_GROUP, MATERIAL_CATEGORY, MATERIAL_TYPE_ID,"
        SQL = SQL & "MATERIAL_NAME, MATERIAL_QUANTITY, MATERIAL_VOLUME, "
        SQL = SQL & "CASE WHEN ITEM_PRICES.PRICE IS NULL THEN 0 ELSE ITEM_PRICES.PRICE END AS MATERIAL_PRICE_PER_UNIT "
        SQL = SQL & "FROM REACTIONS LEFT OUTER JOIN ITEM_PRICES ON REACTIONS.MATERIAL_TYPE_ID  = ITEM_PRICES.ITEM_ID "

        If LoadAdvMoon Then
            ReactionGroupList = ReactionGroupList & "'Complex Reactions',"
        End If
        If LoadSimpleMoon Then
            ReactionGroupList = ReactionGroupList & "'Simple Reaction',"
        End If
        If LoadHybrid Then
            ReactionGroupList = ReactionGroupList & "'Hybrid Reactions',"
        End If
        If LoadSimpleBio Then
            ReactionGroupList = ReactionGroupList & "'Simple Biochemical Reactions',"
        End If
        If LoadAdvBio Then
            ReactionGroupList = ReactionGroupList & "'Complex Biochemical Reactions',"
        End If

        SQL = SQL & "WHERE REACTION_TYPE = 'Output' "
        SQL = SQL & "AND REACTION_GROUP IN (" & ReactionGroupList.Substring(0, Len(ReactionGroupList) - 1) & ") "
        SQL = SQL & "AND MATERIAL_CATEGORY NOT IN ('Commodity','Planetary Commodities')"

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerReactions = DBCommand.ExecuteReader()

        While readerReactions.Read
            CurrentReaction = Nothing

            ' For each resulting reaction, set initial data of the final reaction
            CurrentReaction.TypeID = readerReactions.GetInt32(0)
            CurrentReaction.Reaction = readerReactions.GetString(1)
            CurrentReaction.ReactionGroup = readerReactions.GetString(2)

            ' Set the name of the reaction type here by group type
            Select Case CurrentReaction.ReactionGroup
                Case "Complex Biochemical Reactions"
                    CurrentReaction.ReactionType = "Complex Biochemical"
                Case "Complex Reactions"
                    CurrentReaction.ReactionType = "Advanced Moon Materials"
                Case "Hybrid Reactions"
                    CurrentReaction.ReactionType = "Hybrid Polymers"
                Case "Simple Biochemical Reactions"
                    CurrentReaction.ReactionType = "Simple Biochemical"
                Case "Simple Reaction"
                    CurrentReaction.ReactionType = "Processed Moon Materials"
            End Select

            ' Get the Inputs
            CurrentReaction.Inputs = GetReactionInputs(CurrentReaction.TypeID, BuildBaseMats, IgnoreBaseMatPrice)

            ' Set the output material
            ' If we are refining and the material produced is unrefined, then get refined mats value
            If Refine And readerReactions.GetString(6).Contains("Unrefined") Then
                Dim Tax As Double
                Dim Efficiency As Double

                If cmbReactionsRefiningEfficiency.Text.Contains("%") Then
                    Efficiency = CDbl(cmbReactionsRefiningEfficiency.Text.Substring(0, Len(cmbReactionsRefiningEfficiency.Text) - 1)) / 100
                Else
                    Efficiency = CDbl(cmbReactionsRefiningEfficiency.Text) / 100
                End If

                Dim RefineTax As Double = CDbl(cmbReactionsRefineTax.Text.Replace("%", ""))

                If RefineTax <= 0 Then
                    Tax = 0
                Else
                    Tax = RefineTax / 100
                End If

                ReprocessingStation = New RefiningReprocessing(SelectedCharacter.Skills.GetSkillLevel(3385),
                                                              SelectedCharacter.Skills.GetSkillLevel(3389),
                                                              SelectedCharacter.Skills.GetSkillLevel(12196),
                                                              UserApplicationSettings.RefiningImplantValue, Efficiency,
                                                              Tax, CDbl(txtReactionsRefineryStanding.Text))

                TempMats = ReprocessingStation.ReprocessMaterial(readerReactions.GetInt64(5), 1, 1, False, False, False)
                RefineOutputName = ""
                RefineOutputQuantity = 0

                ' Sum up the outputs
                For k = 0 To TempMats.GetMaterialList.Count - 1
                    ' Save the name/quantity as a combination of the outputs
                    RefineOutputName = RefineOutputName & TempMats.GetMaterialList(k).GetMaterialName & "(" & CStr(TempMats.GetMaterialList(k).GetQuantity) & ") - "
                    RefineOutputQuantity = RefineOutputQuantity + TempMats.GetMaterialList(k).GetQuantity
                    RefineOutputVolume = RefineOutputVolume + TempMats.GetMaterialList(k).GetVolume
                Next

                RefineOutputName = RefineOutputName.Substring(0, Len(RefineOutputName) - 3)

                ' Set the refine output - use the total cost divided by the total quantity to fudge the numbers to look correct in the screen
                CurrentReaction.Output = New Material(readerReactions.GetInt64(5),
                                                   RefineOutputName, readerReactions.GetString(3),
                                                   RefineOutputQuantity, RefineOutputVolume, TempMats.GetTotalMaterialsCost / RefineOutputQuantity, "-", "-")
            Else
                ' Set the output
                CurrentReaction.Output = New Material(readerReactions.GetInt64(5), readerReactions.GetString(6),
                                                          readerReactions.GetString(3), readerReactions.GetInt64(7),
                                                          readerReactions.GetDouble(8), readerReactions.GetDouble(9), "0", "0")
            End If

            ' We are assuming the full chain is set up and processing - so 1 hour per reaction
            ' Unless we are building the base materials, these output 200 units instead of 100 required
            ' for final reaction. So assume the number of POS's entered takes this into account and double the reaction output quantity
            If BuildBaseMats And (CurrentReaction.ReactionGroup = "Complex Biochemical Reactions" Or CurrentReaction.ReactionGroup = "Complex Reactions") Then
                ' Double the output mats
                CurrentReaction.Output.SetQuantity(CurrentReaction.Output.GetQuantity * 2)
            End If

            ' Determine final profit
            CurrentReaction.ProfitPerHour = CurrentReaction.Output.GetTotalCost - CurrentReaction.Inputs.GetTotalMaterialsCost - (TotalHourlyPOSCost * NumberofTowers)

            ' Finally set the taxes and fees
            If SetTaxes Then
                CurrentReaction.Taxes = GetSalesTax(CurrentReaction.ProfitPerHour)
                CurrentReaction.ProfitPerHour = CurrentReaction.ProfitPerHour - CurrentReaction.Taxes
            End If

            If SetFees Then
                CurrentReaction.Fees = GetSalesTax(CurrentReaction.ProfitPerHour)
                CurrentReaction.ProfitPerHour = CurrentReaction.ProfitPerHour - CurrentReaction.Fees
            End If

            FinalReactionList.Add(CurrentReaction)

        End While

        readerReactions.Close()
        DBCommand = Nothing

        Return FinalReactionList

    End Function

    ' Gets the inputs of an output reaction and returns the list of materials
    Private Function GetReactionInputs(ReactionTypeID As Long, BuildBaseMaterials As Boolean, IgnoreBaseMatPrices As Boolean) As Materials
        Dim readerInputs As SQLiteDataReader
        Dim readerSubInput As SQLiteDataReader
        Dim SQL As String
        Dim ReactionInputList As New Materials
        Dim ReactionSubInputList As New Materials
        Dim BaseInputList As New List(Of Material)
        Dim InputMaterial As Material

        SQL = "SELECT REACTION_TYPE_ID, REACTION_NAME, REACTION_GROUP, MATERIAL_GROUP, MATERIAL_CATEGORY, MATERIAL_TYPE_ID,"
        SQL = SQL & "MATERIAL_NAME, MATERIAL_QUANTITY, MATERIAL_VOLUME, "
        SQL = SQL & "CASE WHEN ITEM_PRICES.PRICE IS NULL THEN 0 ELSE ITEM_PRICES.PRICE END AS MATERIAL_PRICE_PER_UNIT "
        SQL = SQL & "FROM REACTIONS LEFT OUTER JOIN ITEM_PRICES ON REACTIONS.MATERIAL_TYPE_ID  = ITEM_PRICES.ITEM_ID "
        SQL = SQL & "WHERE REACTION_TYPE = 'Input' AND REACTION_TYPE_ID = " & ReactionTypeID

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerInputs = DBCommand.ExecuteReader()

        While readerInputs.Read
            ' Two options, buy them or build them if they are complex
            If BuildBaseMaterials And (readerInputs.GetString(2) = "Complex Biochemical Reactions" Or readerInputs.GetString(2) = "Complex Reactions") _
                And readerInputs.GetString(4) <> "Commodity" And readerInputs.GetString(4) <> "Planetary Commodities" Then
                ' Look up each input material for it's base build cost and use that for profit calc
                '        ' The logic for a full chain is as follows:
                '        ' 2 Basic Moon Materials from Moon Mining -> ReactoreaderInputsr => 1 Processed Moon Material} These 2 Processed Moon Materials -> Reactor => Advanced Moon Material
                '        ' 2 Basic Moon Materials from Moon Mining -> Reactor => 1 Processed Moon Material}
                ' 
                ' Assume it takes one hour to do a full chain after one gets the initial materials loaded and running (~3 hour startup)

                ' Get the reaction type id for the input reaction
                SQL = "SELECT REACTION_TYPE_ID, REACTION_GROUP FROM REACTIONS WHERE REACTION_TYPE = 'Output' and MATERIAL_NAME = '" & readerInputs.GetString(6) & "'"

                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerSubInput = DBCommand.ExecuteReader()

                Call readerSubInput.Read()
                ReactionSubInputList = GetReactionInputs(readerSubInput.GetInt64(0), BuildBaseMaterials, IgnoreBaseMatPrices)

                ' Add the inputs to the list
                For i = 0 To ReactionSubInputList.GetMaterialList.Count - 1
                    ReactionSubInputList.GetMaterialList(i).SetTotalCost(0)
                    ReactionInputList.InsertMaterial(ReactionSubInputList.GetMaterialList(i))
                    ' If this is a base material, and they want to ignore cost, then update
                    If IgnoreBaseMatPrices And (readerSubInput.GetString(1) = "Simple Biochemical Reactions" Or readerSubInput.GetString(1) = "Simple Reaction") Then
                        ' Find the material we just insert and set the cost to 0
                        For j = 0 To ReactionInputList.GetMaterialList.Count - 1
                            If ReactionInputList.GetMaterialList(j).GetMaterialName = ReactionSubInputList.GetMaterialList(i).GetMaterialName Then
                                Dim TempPrice As Double
                                TempPrice = ReactionInputList.GetMaterialList(j).GetTotalCost
                                ReactionInputList.GetMaterialList(j).SetTotalCost(0)
                                ReactionInputList.ResetTotalValue(ReactionInputList.GetTotalMaterialsCost - TempPrice)
                            End If
                        Next
                    End If
                Next

                readerSubInput.Close()

            Else ' Buying

                ' Insert all the inputs into the list individually
                InputMaterial = New Material(readerInputs.GetInt64(5), readerInputs.GetString(6),
                                                              readerInputs.GetString(3), readerInputs.GetInt64(7),
                                                              readerInputs.GetDouble(8), readerInputs.GetDouble(9), "0", "0")

                ' If this is a base material, and they want to ignore cost, then update
                If IgnoreBaseMatPrices And (readerInputs.GetString(2) = "Simple Biochemical Reactions" Or readerInputs.GetString(2) = "Simple Reaction") Then
                    Call InputMaterial.SetTotalCost(0)
                End If

                ReactionInputList.InsertMaterial(InputMaterial)

            End If

        End While

        readerInputs.Close()
        DBCommand = Nothing

        Return ReactionInputList

    End Function

    ' Updates the grid with all the reactions
    Public Sub UpdateReactionsGrid()
        Dim lstViewRow As ListViewItem
        Dim POSFuelCost As Double
        Dim NumberOfPOS As Integer
        Dim ReactionIPH As Double
        Dim IgnoreMoonMatPrice As Boolean = False
        Dim BuildBaseMats As Boolean = False
        Dim i As Integer

        ' Don't do anything but clear the list if no items checked
        If Not chkReactionsAdvMoonMats.Checked And Not chkReactionsComplexBio.Checked And Not chkReactionsHybrid.Checked And Not chkReactionsProcMoonMats.Checked And Not chkReactionsSimpleBio.Checked Then
            lstReactions.Items.Clear()
            Exit Sub
        End If

        ' Check Fuel Cost
        If Not IsNumeric(txtReactionPOSFuelCost.Text) Or Trim(txtReactionPOSFuelCost.Text) = "" Then
            MsgBox("Must Enter Fuel Cost", vbExclamation, Me.Text)
            txtReactionPOSFuelCost.Focus()
            Exit Sub
        Else
            POSFuelCost = CDbl(txtReactionPOSFuelCost.Text)
        End If

        If Not IsNumeric(txtReactionPOSFuelCost.Text) Or Trim(txtReactionsNumPOS.Text) = "" Then
            MsgBox("Invalid Number of POSs", vbExclamation, Me.Text)
            txtReactionsNumPOS.Focus()
            Exit Sub
        Else
            NumberOfPOS = CInt(txtReactionsNumPOS.Text)
        End If

        If Not IsNumeric(txtReactionsRefineryStanding.Text) Or Trim(txtReactionsRefineryStanding.Text) = "" Then
            MsgBox("Invalid Number of POSs", vbExclamation, Me.Text)
            txtReactionsRefineryStanding.Focus()
            Exit Sub
        End If

        If Val(txtReactionsRefineryStanding.Text) > 10 Then
            MsgBox("Choose a lower value for Refinery Standing", vbExclamation, Me.Text)
            txtReactionsRefineryStanding.Focus()
            Exit Sub
        End If

        ' Working
        Me.Cursor = Cursors.WaitCursor

        If chkReactionsIgnoreBaseMatPrice.Checked And chkReactionsIgnoreBaseMatPrice.Enabled Then
            IgnoreMoonMatPrice = True
        Else
            IgnoreMoonMatPrice = False
        End If

        If chkReactionsBuildBasic.Checked And chkReactionsBuildBasic.Enabled Then
            BuildBaseMats = True
        Else
            BuildBaseMats = False
        End If

        ' Get the reaction list
        GlobalReactionList = BuildReactionList(chkReactionsAdvMoonMats.Checked, chkReactionsProcMoonMats.Checked, chkReactionsHybrid.Checked,
                                            chkReactionsSimpleBio.Checked, chkReactionsComplexBio.Checked, IgnoreMoonMatPrice, BuildBaseMats,
                                            chkReactionsTaxes.Checked, chkReactionsFees.Checked, chkReactionsRefine.Checked, POSFuelCost, NumberOfPOS)

        If GlobalReactionList.Count > 0 Then
            ' Sort it
            GlobalReactionList.Sort(New ReactionIPHComparer)

            ' Put the materials in the list
            ' Clear List and begin update
            lstReactions.BeginUpdate()
            lstReactions.Items.Clear()

            For i = 0 To GlobalReactionList.Count - 1
                lstViewRow = New ListViewItem(GlobalReactionList(i).ReactionType)
                'The remaining columns are subitems  
                lstViewRow.SubItems.Add(GlobalReactionList(i).Reaction)
                lstViewRow.SubItems.Add(GlobalReactionList(i).Output.GetMaterialName)
                lstViewRow.SubItems.Add(FormatNumber(GlobalReactionList(i).Output.GetQuantity, 0))
                lstViewRow.SubItems.Add(GlobalReactionList(i).Output.GetMaterialGroup)
                ReactionIPH = GlobalReactionList(i).ProfitPerHour

                If ReactionIPH < 0 Then
                    lstViewRow.ForeColor = Color.Red
                Else
                    lstViewRow.ForeColor = Color.Black
                End If

                ' Color row by type of reaction
                Select Case GlobalReactionList(i).Output.GetMaterialGroup
                    Case "Biochemical Material"
                        If GlobalReactionList(i).ReactionGroup.Substring(0, 6) = "Simple" Then
                            lstViewRow.BackColor = Color.LightYellow
                        Else
                            lstViewRow.BackColor = Color.LightGreen
                        End If
                    Case "Composite"
                        lstViewRow.BackColor = Color.Wheat
                    Case "Hybrid Polymers"
                        lstViewRow.BackColor = Color.LightSteelBlue
                    Case "Intermediate Materials"
                        lstViewRow.BackColor = Color.LightCyan
                End Select

                lstViewRow.SubItems.Add(FormatNumber(ReactionIPH, 2)) ' IPH

                Call lstReactions.Items.Add(lstViewRow)

            Next

            ' Now sort this
            Dim TempType As SortOrder
            If ReactionsColumnSortType = SortOrder.Ascending Then
                TempType = SortOrder.Descending
            Else
                TempType = SortOrder.Ascending
            End If
            Call ListViewColumnSorter(ReactionsColumnClicked, CType(lstReactions, ListView), ReactionsColumnClicked, TempType)
            Me.Cursor = Cursors.Default

            lstReactions.EndUpdate()
        End If

        Me.Cursor = Cursors.Default

    End Sub

    ' Loads the reaction into the list
    Private Sub LoadReaction(ByVal SentReaction As Reaction)
        Dim lstViewRow As ListViewItem
        Dim ReactionIPH As Double

        lstViewRow = New ListViewItem(SentReaction.ReactionType)
        'The remaining columns are subitems  
        lstViewRow.SubItems.Add(SentReaction.Reaction)
        lstViewRow.SubItems.Add(SentReaction.Output.GetMaterialName)
        lstViewRow.SubItems.Add(SentReaction.Output.GetMaterialGroup)
        ReactionIPH = SentReaction.ProfitPerHour - 15000

        If ReactionIPH < 0 Then
            lstViewRow.ForeColor = Color.Red
        Else
            lstViewRow.ForeColor = Color.Black
        End If

        ' Color row by type of reaction
        Select Case SentReaction.Output.GetMaterialGroup
            Case "Biochemical Material"
                If SentReaction.ReactionGroup.Substring(0, 6) = "Simple" Then
                    lstViewRow.BackColor = Color.LightYellow
                Else
                    lstViewRow.BackColor = Color.LightGreen
                End If
            Case "Composite"
                lstViewRow.BackColor = Color.Wheat
            Case "Hybrid Polymers"
                lstViewRow.BackColor = Color.LightSteelBlue
            Case "Intermediate Materials"
                lstViewRow.BackColor = Color.LightCyan
        End Select

        lstViewRow.SubItems.Add(FormatNumber(ReactionIPH, 2))

        Call lstReactions.Items.Add(lstViewRow)

    End Sub

    ' Function takes 2 reaction lists and returns the combined list
    'Private Function AddReactions(ByVal ReactionList1 As Reaction(), ByVal ReactionList2 As Reaction()) As Reaction()
    '    Dim TempReactions() As Reaction
    '    Dim i As Integer = 0
    '    Dim j As Integer = 0
    '    Dim TotalRecords As Integer

    '    If IsNothing(ReactionList1) And IsNothing(ReactionList2) Then
    '        Return Nothing
    '    ElseIf IsNothing(ReactionList1) Then
    '        Return ReactionList2
    '    ElseIf IsNothing(ReactionList2) Then
    '        Return ReactionList1
    '    End If

    '    TotalRecords = ReactionList1.Count + ReactionList2.Count - 1
    '    ReDim TempReactions(TotalRecords)

    '    ' Copy the first list
    '    For i = 0 To ReactionList1.Count - 1
    '        TempReactions(i).TypeID = ReactionList1(i).TypeID
    '        TempReactions(i).ReactionType = ReactionList1(i).ReactionType
    '        TempReactions(i).Reaction = ReactionList1(i).Reaction
    '        TempReactions(i).ReactionGroup = ReactionList1(i).ReactionGroup
    '        TempReactions(i).Output = ReactionList1(i).Output
    '        TempReactions(i).Inputs = ReactionList1(i).Inputs
    '        TempReactions(i).ProfitPerHour = ReactionList1(i).ProfitPerHour
    '        TempReactions(i).Fees = ReactionList1(i).Fees
    '        TempReactions(i).Taxes = ReactionList1(i).Taxes
    '    Next

    '    'Now the second
    '    For i = ReactionList1.Count To TotalRecords
    '        TempReactions(i).TypeID = ReactionList2(j).TypeID
    '        TempReactions(i).ReactionType = ReactionList2(j).ReactionType
    '        TempReactions(i).Reaction = ReactionList2(j).Reaction
    '        TempReactions(i).ReactionGroup = ReactionList2(j).ReactionGroup
    '        TempReactions(i).Output = ReactionList2(j).Output
    '        TempReactions(i).Inputs = ReactionList2(j).Inputs
    '        TempReactions(i).ProfitPerHour = ReactionList2(j).ProfitPerHour
    '        TempReactions(i).Fees = ReactionList2(j).Fees
    '        TempReactions(i).Taxes = ReactionList2(j).Taxes
    '        j = j + 1
    '    Next

    '    Return TempReactions

    'End Function

    ' Sorts a Reaction list descending
    'Private Sub SortListDesc(ByRef List() As Reaction, ByVal First As Integer, ByVal Last As Integer)
    '    Dim LowIndex As Integer
    '    Dim HighIndex As Integer
    '    Dim MidValue As Double

    '    ' Quicksort
    '    LowIndex = First
    '    HighIndex = Last
    '    MidValue = List((First + Last) \ 2).ProfitPerHour

    '    Do
    '        While List(LowIndex).ProfitPerHour > MidValue
    '            LowIndex = LowIndex + 1
    '        End While

    '        While List(HighIndex).ProfitPerHour < MidValue
    '            HighIndex = HighIndex - 1
    '        End While

    '        If LowIndex <= HighIndex Then
    '            Swap(List, LowIndex, HighIndex)
    '            LowIndex = LowIndex + 1
    '            HighIndex = HighIndex - 1
    '        End If

    '    Loop While LowIndex <= HighIndex

    '    If First < HighIndex Then
    '        SortListDesc(List, First, HighIndex)
    '    End If

    '    If LowIndex < Last Then
    '        SortListDesc(List, LowIndex, Last)
    '    End If

    'End Sub

    ' This swaps the Reaction list values
    'Private Sub Swap(ByRef List() As Reaction, ByRef IndexA As Integer, ByRef IndexB As Integer)
    '    Dim Temp As Reaction

    '    Temp = List(IndexA)
    '    List(IndexA) = List(IndexB)
    '    List(IndexB) = Temp

    'End Sub

    ' Updates the input list boxes when a row is clicked
    Private Sub lstReactions_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstReactions.SelectedIndexChanged
        Dim i As Integer
        Dim index As Integer
        Dim lstViewRow As ListViewItem

        ' Load up the inputs into the boxes depending on number of mats
        ' Reactions is form global and since it is sorted, it represents what is in lstReactions
        If lstReactions.SelectedItems.Count > 0 Then
            index = lstReactions.SelectedIndices(0)

            ' Look up the index in the global list
            index = GetGlobalReactionListIndex(lstReactions.SelectedItems(0).SubItems(1).Text)

            lstReactionMats.BeginUpdate()
            lstReactionMats.Items.Clear()

            If Not IsNothing(GlobalReactionList(index).Inputs.GetMaterialList) Then
                For i = 0 To GlobalReactionList(index).Inputs.GetMaterialList.Count - 1

                    lstViewRow = New ListViewItem(GlobalReactionList(index).Inputs.GetMaterialList(i).GetMaterialName)
                    'The remaining columns are subitems  
                    'lstViewRow.SubItems.Add(GlobalReactionList(index).Inputs.GetMaterialList(i).GetCostPerItem)
                    lstViewRow.SubItems.Add(CStr(GlobalReactionList(index).Inputs.GetMaterialList(i).GetQuantity))
                    Call lstReactionMats.Items.Add(lstViewRow)
                Next

                ' Populate Taxes and Fees
                lblReactionsFees.Text = FormatNumber(GlobalReactionList(index).Fees, 2)
                lblReactionsTaxes.Text = FormatNumber(GlobalReactionList(index).Taxes, 2)

            End If
            lstReactionMats.EndUpdate()
        End If

        lstReactionMats.Visible = True

    End Sub

    ' Searches through the reaction list for the selected reaction and returns the index in that list
    ' This is because sorting the list doesn't update the reaction list (Temp Fix)
    Private Function GetGlobalReactionListIndex(ReactionName As String) As Integer

        For i = 0 To GlobalReactionList.Count - 1
            If GlobalReactionList(i).Reaction = ReactionName Then
                Return i
            End If
        Next

        Return 0

    End Function

    ' Reaction Structure
    Public Structure Reaction
        Dim TypeID As Integer
        Dim ReactionType As String
        Dim Reaction As String
        Dim ReactionGroup As String
        Dim Output As Material
        Dim ProfitPerHour As Double
        Dim Taxes As Double ' Taxes for selling this item
        Dim Fees As Double ' Fees for setting up sell order
        ' Inputs
        Dim Inputs As Materials
    End Structure

    ' For sorting a list of Reactions
    Public Class ReactionIPHComparer

        Implements System.Collections.Generic.IComparer(Of Reaction)

        Public Function Compare(ByVal p1 As Reaction, ByVal p2 As Reaction) As Integer Implements IComparer(Of Reaction).Compare
            ' swap p2 and p1 to do decending sort
            Return p2.ProfitPerHour.CompareTo(p1.ProfitPerHour)
        End Function

    End Class

#End Region

#Region "Mining"

#Region "Mining Object Functions"

    Private Sub lstMineGrid_ColumnWidthChanging(sender As Object, e As System.Windows.Forms.ColumnWidthChangingEventArgs) Handles lstMineGrid.ColumnWidthChanging
        If e.ColumnIndex = 0 Then
            e.Cancel = True
            e.NewWidth = lstPricesView.Columns(e.ColumnIndex).Width
        End If
    End Sub

    Private Sub lstMineGrid_MouseClick(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles lstMineGrid.MouseClick
        Call ListClicked(lstMineGrid, sender, e)
    End Sub

    Private Sub chkOreProcessing1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing1.CheckedChanged
        Call UpdateProcessingSkillBoxes(1, chkOreProcessing1.Checked)
    End Sub

    Private Sub chkOreProcessing2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing2.CheckedChanged
        Call UpdateProcessingSkillBoxes(2, chkOreProcessing2.Checked)
    End Sub

    Private Sub chkOreProcessing3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing3.CheckedChanged
        Call UpdateProcessingSkillBoxes(3, chkOreProcessing3.Checked)
    End Sub

    Private Sub chkOreProcessing4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing4.CheckedChanged
        Call UpdateProcessingSkillBoxes(4, chkOreProcessing4.Checked)
    End Sub

    Private Sub chkOreProcessing5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing5.CheckedChanged
        Call UpdateProcessingSkillBoxes(5, chkOreProcessing5.Checked)
    End Sub

    Private Sub chkOreProcessing6_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing6.CheckedChanged
        Call UpdateProcessingSkillBoxes(6, chkOreProcessing6.Checked)
    End Sub

    Private Sub chkOreProcessing7_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing7.CheckedChanged
        Call UpdateProcessingSkillBoxes(7, chkOreProcessing7.Checked)
    End Sub

    Private Sub chkOreProcessing8_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing8.CheckedChanged
        Call UpdateProcessingSkillBoxes(8, chkOreProcessing8.Checked)
    End Sub

    Private Sub chkOreProcessing17_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing17.CheckedChanged
        Call UpdateProcessingSkillBoxes(17, chkOreProcessing17.Checked)
    End Sub

    Private Sub chkOreProcessing9_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing9.CheckedChanged
        Call UpdateProcessingSkillBoxes(9, chkOreProcessing9.Checked)
    End Sub

    Private Sub chkOreProcessing10_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing10.CheckedChanged
        Call UpdateProcessingSkillBoxes(10, chkOreProcessing10.Checked)
    End Sub

    Private Sub chkOreProcessing11_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing11.CheckedChanged
        Call UpdateProcessingSkillBoxes(11, chkOreProcessing11.Checked)
    End Sub

    Private Sub chkOreProcessing12_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing12.CheckedChanged
        Call UpdateProcessingSkillBoxes(12, chkOreProcessing12.Checked)
    End Sub

    Private Sub chkOreProcessing13_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing13.CheckedChanged
        Call UpdateProcessingSkillBoxes(13, chkOreProcessing13.Checked)
    End Sub

    Private Sub chkOreProcessing14_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing14.CheckedChanged
        Call UpdateProcessingSkillBoxes(14, chkOreProcessing14.Checked)
    End Sub

    Private Sub chkOreProcessing15_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing15.CheckedChanged
        Call UpdateProcessingSkillBoxes(15, chkOreProcessing15.Checked)
    End Sub

    Private Sub chkOreProcessing16_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOreProcessing16.CheckedChanged
        Call UpdateProcessingSkillBoxes(16, chkOreProcessing16.Checked)
    End Sub

    Private Sub UpdateProcessingSkillBoxes(ByVal Index As Integer, ByVal Checked As Boolean)
        MineProcessingCombos(Index).Enabled = Checked
        MineProcessingLabels(Index).Enabled = Checked
    End Sub

    Private Sub cmbMineShipType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineShipType.SelectedIndexChanged
        If Not UpdatingMiningShips And Not FirstLoad Then
            Call LoadMiningshipImage()
            Call UpdateMiningSkills()
            Call UpdateMiningShipEquipment()
            ' Clear the grid
            lstMineGrid.Items.Clear()
        End If
    End Sub

    Private Sub cmbMineShipType_DropDown(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbMineShipType.DropDown
        Call UpdateMiningShipsCombo()
    End Sub

    Private Sub cmbMineAstrogeology_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineAstrogeology.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateMiningShipForm(True)
        End If
    End Sub

    Private Sub cmbMineExhumers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineExhumers.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateMiningShipForm(False)
        End If
    End Sub

    Private Sub cmbMineBaseShipSkill_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineBaseShipSkill.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateMiningShipForm(False)
        End If
    End Sub

    Private Sub cmbMineSkill_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineSkill.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateMiningShipForm(True)
        End If
    End Sub

    Private Sub cmbMineIceHarvesting_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineGasIceHarvesting.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateMiningShipForm(True)
        End If
    End Sub

    Private Sub cmbMineDeepCore_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineDeepCore.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateMiningShipForm(True)
        End If
    End Sub

    Private Sub cmbMineType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineOreType.SelectedIndexChanged

        If cmbMineOreType.Text = "Ice" Then
            chkMineIncludeHighYieldOre.Enabled = False
            gbMineOreProcessingType.Enabled = True
            chkMineIncludeHighYieldOre.Text = "High Yield Ice"
            chkMineIncludeHighSec.Text = "High Sec Ice"
            chkMineIncludeLowSec.Text = "Low Sec Ice"
            chkMineIncludeNullSec.Text = "Null Sec Ice"
            lstMineGrid.Columns(1).Text = "Ice Name"
            gbMineMiningDroneM3.Enabled = False ' drones don't apply to ice
            rbtnMineIceRig.Enabled = True
            If UserMiningTabSettings.IceMiningRig Then
                rbtnMineIceRig.Checked = True
            Else
                rbtnMineNoRigs.Checked = True
            End If
            rbtnMineMercoxitRig.Enabled = False

            ' No ice in wormholes
            chkMineWH.Enabled = False
            chkMineC1.Enabled = False
            chkMineC2.Enabled = False
            chkMineC3.Enabled = False
            chkMineC4.Enabled = False
            chkMineC5.Enabled = False
            chkMineC6.Enabled = False

            If chkMineRefinedOre.Checked Then
                gbMineBaseRefineSkills.Enabled = True
                gbMineStationYield.Enabled = True
            End If

            gbMineRefining.Enabled = True

        ElseIf cmbMineOreType.Text = "Ore" Then
            chkMineIncludeHighYieldOre.Enabled = True
            gbMineOreProcessingType.Enabled = True
            chkMineIncludeHighYieldOre.Text = "High Yield Ores"
            chkMineIncludeHighSec.Text = "High Sec Ore"
            chkMineIncludeLowSec.Text = "Low Sec Ore"
            chkMineIncludeNullSec.Text = "Null Sec Ore"
            lstMineGrid.Columns(1).Text = "Ore Name"
            gbMineMiningDroneM3.Enabled = True
            rbtnMineMercoxitRig.Enabled = True
            rbtnMineIceRig.Enabled = False
            If UserMiningTabSettings.MercoxitMiningRig Then
                rbtnMineMercoxitRig.Checked = True
            Else
                rbtnMineNoRigs.Checked = True
            End If

            chkMineWH.Enabled = True
            chkMineC1.Enabled = True
            chkMineC2.Enabled = True
            chkMineC3.Enabled = True
            chkMineC4.Enabled = True
            chkMineC5.Enabled = True
            chkMineC6.Enabled = True

            If chkMineRefinedOre.Checked Then
                gbMineBaseRefineSkills.Enabled = True
                gbMineStationYield.Enabled = True
            End If

            gbMineRefining.Enabled = True

        ElseIf cmbMineOreType.Text = "Gas" Then
            chkMineIncludeHighYieldOre.Enabled = False
            gbMineOreProcessingType.Enabled = False
            chkMineIncludeHighYieldOre.Text = "High Yield Gas"
            chkMineIncludeHighSec.Text = "High Sec Gas"
            chkMineIncludeLowSec.Text = "Low Sec Gas"
            chkMineIncludeNullSec.Text = "Null Sec Gas"
            lstMineGrid.Columns(1).Text = "Gas Name"
            gbMineMiningDroneM3.Enabled = False
            rbtnMineMercoxitRig.Enabled = False
            rbtnMineIceRig.Enabled = False
            rbtnMineNoRigs.Checked = True

            chkMineWH.Enabled = True
            chkMineC1.Enabled = True
            chkMineC2.Enabled = True
            chkMineC3.Enabled = True
            chkMineC4.Enabled = True
            chkMineC5.Enabled = True
            chkMineC6.Enabled = True

            ' No refining for Gas
            gbMineBaseRefineSkills.Enabled = False
            gbMineStationYield.Enabled = False
            gbMineRefining.Enabled = False

        End If

        If Not FirstLoad Then
            ' Load all the skills for the character first
            Call LoadCharacterMiningSkills()
            Call UpdateMiningImplants()
            Call UpdateMiningShipForm(True)
            Call UpdateProcessingSkills()
        End If

        lstMineGrid.Items.Clear()

    End Sub

    Private Sub lstMineGrid_ColumnClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstMineGrid.ColumnClick
        Call ListViewColumnSorter(e.Column, lstMineGrid, MiningColumnClicked, MiningColumnSortType)
    End Sub

    Private Sub chkMineUseFleetBooster_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMineUseFleetBooster.CheckedChanged
        If Not FirstLoad Then
            Call LoadFleetBoosterImage()
            Call UpdateBoosterSkills()
        End If
    End Sub

    Private Sub cmbMineMiningForeman_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineMiningForeman.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateBoosterSkills()
        End If
    End Sub

    Private Sub cmbMineMiningDirector_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineMiningDirector.SelectedIndexChanged
        If Not FirstLoad Then
            Call UpdateBoosterSkills()
        End If
    End Sub

    Private Sub cmbMineBoosterShip_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineBoosterShip.SelectedIndexChanged
        If Not UpdatingMiningShips Then
            Call LoadFleetBoosterImage()
            Call UpdateBoosterSkills()
            ' Clear the grid
            lstMineGrid.Items.Clear()
        End If
    End Sub

    Private Sub chkMineForemanBooster_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkMineForemanLaserOpBoost.Click
        Call UpdateMiningBoosterObjects()
    End Sub

    Private Sub txtMineNumberMiners_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineNumberMiners.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtMineTotalJumpM3_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineTotalJumpM3.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtMineTotalJumpFuel_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineTotalJumpFuel.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub chkMineUseHauler_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMineUseHauler.CheckedChanged
        If chkMineUseHauler.Checked Then
            lblMineRTMin.Enabled = False
            txtMineRTMin.Enabled = False
            lblMineRTSec.Enabled = False
            txtMineRTSec.Enabled = False
            lblMineHaulerM3.Enabled = False
            txtMineHaulerM3.Enabled = False
        Else
            lblMineRTMin.Enabled = True
            txtMineRTMin.Enabled = True
            lblMineRTSec.Enabled = True
            txtMineRTSec.Enabled = True
            lblMineHaulerM3.Enabled = True
            txtMineHaulerM3.Enabled = True
        End If

        ' Refresh this value regardless
        Call RefreshHaulerM3()

    End Sub

    Private Sub txtMineRTMin_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMineRTMin.GotFocus
        Call txtMineRTMin.SelectAll()
    End Sub

    Private Sub txtMineHaulerM3_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMineHaulerM3.GotFocus
        Call txtMineHaulerM3.SelectAll()
    End Sub

    Private Sub txtMineTotalJumpM3_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMineTotalJumpM3.GotFocus
        Call txtMineTotalJumpM3.SelectAll()
    End Sub

    Private Sub txtMineTotalJumpFuel_GotFocus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMineTotalJumpFuel.GotFocus
        Call txtMineTotalJumpFuel.SelectAll()
    End Sub

    Private Sub txtMineHaulerM3_KeyPress(sender As System.Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineHaulerM3.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtMineHaulerM3_LostFocus(sender As Object, e As System.EventArgs) Handles txtMineHaulerM3.LostFocus
        txtMineHaulerM3.Text = FormatNumber(txtMineHaulerM3.Text, 1)
    End Sub

    Private Sub txtMineRTMin_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineRTMin.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtMineRTSec_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMineRTSec.GotFocus
        Call txtMineRTSec.SelectAll()
    End Sub

    Private Sub txtMineRTSec_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineRTSec.KeyPress
        ' Only allow numbers or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedRunschars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub txtMineRTSec_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtMineRTSec.TextChanged
        ' Update the minutes and seconds if they enter greater than 60
        If CInt(txtMineRTSec.Text) >= 60 Then
            txtMineRTMin.Text = CStr(Math.Floor(CInt(txtMineRTSec.Text) / 60))
            txtMineRTSec.Text = CStr(CInt(txtMineRTSec.Text) - (CInt(txtMineRTMin.Text) * 60))
        End If
    End Sub

    Private Sub txtMineMiningDroneM3_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineMiningDroneM3.KeyPress
        ' Only allow numbers, decmial or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPriceChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub btnMineRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMineRefresh.Click
        Call LoadMiningGrid()
    End Sub

    Private Sub btnMineReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMineReset.Click
        Call LoadMiningTab()
    End Sub

    Private Sub chkMineIncludeJumpCosts_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMineIncludeJumpCosts.CheckedChanged
        If chkMineIncludeJumpCosts.Checked Then
            lblMineTotalJumpFuel.Enabled = True
            txtMineTotalJumpFuel.Enabled = True
            lblMineTotalJumpM3.Enabled = True
            txtMineTotalJumpM3.Enabled = True
            rbtnMineJumpCompress.Enabled = True
            If chkMineRefinedOre.Checked And chkMineRefinedOre.Enabled Then
                rbtnMineJumpMinerals.Enabled = True
            Else
                rbtnMineJumpMinerals.Enabled = False
            End If
        Else
            lblMineTotalJumpFuel.Enabled = False
            txtMineTotalJumpFuel.Enabled = False
            lblMineTotalJumpM3.Enabled = False
            txtMineTotalJumpM3.Enabled = False
            rbtnMineJumpCompress.Enabled = False
            rbtnMineJumpMinerals.Enabled = False
        End If
    End Sub

    Private Sub cmbMineRefining_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineRefining.SelectedIndexChanged

        ' Load up the right processing checks
        Call UpdateProcessingSkills()

    End Sub

    Private Sub cmbMineRefineryEff_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineRefineryEff.SelectedIndexChanged

        ' Load up the right processing checks
        Call UpdateProcessingSkills()

    End Sub

    Private Sub cmbMineMiningLaser_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbMineMiningLaser.SelectedIndexChanged
        Select Case cmbMineMiningLaser.Text
            Case "Modulated Deep Core Miner II", "Modulated Deep Core Strip Miner II", "Modulated Strip Miner II"
                gbMineCrystals.Enabled = True
                rbtnMineT1Crystals.Enabled = True
                rbtnMineT2Crystals.Enabled = True
            Case Else
                gbMineCrystals.Enabled = False
                rbtnMineT1Crystals.Enabled = False
                rbtnMineT2Crystals.Enabled = False
        End Select
    End Sub

    Private Sub cmbMineRefineStationTax_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbMineRefineStationTax.GotFocus
        Call cmbMineRefineStationTax.SelectAll()
    End Sub

    Private Sub txtMineRefineStanding_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMineRefineStanding.GotFocus
        Call txtMineRefineStanding.SelectAll()
    End Sub

    Private Sub txtMineRefineStanding_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMineRefineStanding.KeyPress
        ' Only allow numbers, decmial or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedDecimalChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub cmbMineRefineStationTax_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles cmbMineRefineStationTax.KeyPress
        ' Only allow numbers, decimal, percent or backspace
        If e.KeyChar <> ControlChars.Back Then
            If allowedPercentChars.IndexOf(e.KeyChar) = -1 Then
                ' Invalid Character
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub chkMineRefinedOre_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineRefinedOre.CheckedChanged
        Call SetOreRefineChecks()
    End Sub

    Private Sub chkMineUnrefinedOre_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineUnrefinedOre.CheckedChanged
        Call SetOreRefineChecks()
    End Sub

    Private Sub chkMineCompressedOre_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineCompressedOre.CheckedChanged
        Call SetOreRefineChecks()
    End Sub

    Private Sub chkMineForemanLaserRangeBoost_Click(sender As Object, e As System.EventArgs) Handles chkMineForemanLaserRangeBoost.Click
        Call UpdateMiningBoosterObjects()
    End Sub

    Private Sub chkMineRorqDeployedMode_Click(sender As Object, e As EventArgs) Handles chkMineRorqDeployedMode.Click
        Call UpdateIndustrialCoreCheck()
    End Sub

    Private Function GetMiningShipImage(ShipName As String) As String
        Dim ImageFile As Long
        Dim BPImage As String

        ' Display the mining ship
        Select Case ShipName
            Case Venture
                ImageFile = MiningShipTypeID.Venture
            Case Covetor
                ImageFile = MiningShipTypeID.Covetor
            Case Retriever
                ImageFile = MiningShipTypeID.Retriever
            Case Hulk
                ImageFile = MiningShipTypeID.Hulk
            Case Skiff
                ImageFile = MiningShipTypeID.Skiff
            Case Procurer
                ImageFile = MiningShipTypeID.Procurer
            Case Mackinaw
                ImageFile = MiningShipTypeID.Mackinaw
            Case Rorqual
                ImageFile = MiningShipTypeID.Rorqual
            Case Orca
                ImageFile = MiningShipTypeID.Orca
            Case Porpoise
                ImageFile = MiningShipTypeID.Porpoise
            Case Drake
                ImageFile = MiningShipTypeID.Drake
            Case Rokh
                ImageFile = MiningShipTypeID.Rokh
            Case Prospect
                ImageFile = MiningShipTypeID.Prospect
            Case Endurance
                ImageFile = MiningShipTypeID.Endurance
            Case Else
                ImageFile = 0
        End Select

        BPImage = Path.Combine(UserImagePath, CStr(ImageFile) & "_64.png")

        Return BPImage

    End Function

    Private Sub chkMineWH_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineWH.CheckedChanged
        If chkMineWH.Checked = True Then
            chkMineC1.Enabled = True
            chkMineC2.Enabled = True
            chkMineC3.Enabled = True
            chkMineC4.Enabled = True
            chkMineC5.Enabled = True
            chkMineC6.Enabled = True
            ' Also check the null box if not checked
            chkMineIncludeNullSec.Checked = True
        Else
            chkMineC1.Enabled = False
            chkMineC2.Enabled = False
            chkMineC3.Enabled = False
            chkMineC4.Enabled = False
            chkMineC5.Enabled = False
            chkMineC6.Enabled = False
        End If

        Call UpdateOrebySpaceChecks()
    End Sub

    Private Sub UpdateOrebySpaceChecks()

        If Not FirstLoad Then
            If cmbMineOreType.Text = "Ore" And chkMineWH.Checked = True And chkMineAmarr.Checked = False And chkMineGallente.Checked = False _
                And chkMineMinmatar.Checked = False And chkMineCaldari.Checked = False Then
                chkMineIncludeHighYieldOre.Enabled = False
            Else
                chkMineIncludeHighYieldOre.Enabled = True
            End If
        End If

    End Sub

    Private Sub chkMineAmarr_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineAmarr.CheckedChanged
        Call UpdateOrebySpaceChecks()
    End Sub

    Private Sub chkMineGallente_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineGallente.CheckedChanged
        Call UpdateOrebySpaceChecks()
    End Sub

    Private Sub chkMineCaldari_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineCaldari.CheckedChanged
        Call UpdateOrebySpaceChecks()
    End Sub

    Private Sub chkMineMinmatar_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineMinmatar.CheckedChanged
        Call UpdateOrebySpaceChecks()
    End Sub

    Private Sub chkMineIncludeNullSec_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkMineIncludeNullSec.CheckedChanged
        ' Don't let them choose WH classes unless null checked
        If chkMineIncludeNullSec.Checked And chkMineWH.Checked Then
            chkMineC1.Enabled = True
            chkMineC2.Enabled = True
            chkMineC3.Enabled = True
            chkMineC4.Enabled = True
            chkMineC5.Enabled = True
            chkMineC6.Enabled = True
        Else
            chkMineC1.Enabled = False
            chkMineC2.Enabled = False
            chkMineC3.Enabled = False
            chkMineC4.Enabled = False
            chkMineC5.Enabled = False
            chkMineC6.Enabled = False
            ' Turn off WH
            chkMineWH.Checked = False
        End If

    End Sub

#End Region

    Private Const AstrogeologySkillTypeID As Integer = 3410
    Private Const DeepCoreMiningSkillTypeID As Integer = 11395
    Private Const ExhumersSkillTypeID As Integer = 22551
    Private Const ExpeditionFrigatesSkillTypeID As Integer = 33856
    Private Const GasCloudHarvestingSkillTypeID As Integer = 25544
    Private Const IceHarvestingSkillTypeID As Integer = 16281
    Private Const MiningSkillTypeID As Integer = 3386
    Private Const MiningBargeSkillTypeID As Integer = 17940
    Private Const MiningFrigateSkillTypeID As Integer = 32918
    Private Const ReprocessingSkillTypeID As Integer = 3385
    Private Const ReprocessingEfficiencySkillTypeID As Integer = 3389

    Private Enum MiningShipTypeID
        'Bantam = 582
        'Tormentor = 591
        'Navitas = 592
        'Burst = 599
        'Osprey = 620
        'Scythe = 631
        Venture = 32880
        Covetor = 17476
        Retriever = 17478
        Hulk = 22544
        Skiff = 22546
        Mackinaw = 22548
        Rorqual = 28352
        Orca = 28606
        Porpoise = 42244
        Procurer = 17480
        Drake = 24698
        Rokh = 24688
        Prospect = 33697
        Endurance = 37135
    End Enum

    Private Sub InitMiningTab()
        Call LoadMiningTab()
    End Sub

    ' Loads in all the skills and such for the tab 
    Private Sub LoadMiningTab()
        Dim i As Integer
        Dim TempSkillLevel As Integer

        With UserMiningTabSettings
            ' Ore types
            cmbMineOreType.Text = .OreType

            chkMineIncludeHighYieldOre.Checked = .CheckHighYieldOres
            chkMineIncludeHighSec.Checked = .CheckHighSecOres
            chkMineIncludeLowSec.Checked = .CheckLowSecOres
            chkMineIncludeNullSec.Checked = .CheckNullSecOres
            chkMineWH.Checked = .CheckSovWormhole

            If chkMineIncludeNullSec.Checked Then
                chkMineC1.Enabled = True
                chkMineC2.Enabled = True
                chkMineC3.Enabled = True
                chkMineC4.Enabled = True
                chkMineC5.Enabled = True
                chkMineC6.Enabled = True
            Else
                chkMineC1.Enabled = False
                chkMineC2.Enabled = False
                chkMineC3.Enabled = False
                chkMineC4.Enabled = False
                chkMineC5.Enabled = False
                chkMineC6.Enabled = False
                ' Can't check wh if they don't select null
                chkMineWH.Checked = False
            End If

            ' Check all locations
            chkMineAmarr.Checked = .CheckSovAmarr
            chkMineCaldari.Checked = .CheckSovCaldari
            chkMineGallente.Checked = .CheckSovGallente
            chkMineMinmatar.Checked = .CheckSovMinmatar

            chkMineC1.Checked = .CheckSovC1
            chkMineC2.Checked = .CheckSovC2
            chkMineC3.Checked = .CheckSovC3
            chkMineC4.Checked = .CheckSovC4
            chkMineC5.Checked = .CheckSovC5
            chkMineC6.Checked = .CheckSovC6

            ' Drones
            txtMineMiningDroneM3.Text = FormatNumber(.MiningDroneM3perHour, 2)

            ' Fleet booster
            chkMineUseFleetBooster.Checked = .CheckUseFleetBooster
            cmbMineBoosterShip.Text = .BoosterShip
            cmbMineMiningDirector.Text = CStr(.MiningDirectorSkill)
            cmbMineMiningForeman.Text = CStr(.MiningFormanSkill)
            cmbMineBoosterShipSkill.Text = CStr(.BoosterShipSkill)
            cmbMineWarfareLinkSpec.Text = CStr(.WarfareLinkSpecSkill)
            chkMineForemanMindlink.Checked = .CheckMiningForemanMindLink
            cmbMineIndustReconfig.Text = CStr(.IndustrialReconfig)

            Select Case .CheckRorqDeployed
                Case 2
                    chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate
                Case 1
                    chkMineRorqDeployedMode.Checked = True
                Case 0
                    chkMineRorqDeployedMode.Checked = False
            End Select

            Call UpdateIndustrialCoreCheck()

            Select Case .CheckMineForemanLaserOpBoost
                Case 2
                    chkMineForemanLaserOpBoost.CheckState = CheckState.Indeterminate
                Case 1
                    chkMineForemanLaserOpBoost.Checked = True
                Case 0
                    chkMineForemanLaserOpBoost.Checked = False
            End Select

            Select Case .CheckMineForemanLaserRangeBoost
                Case 2
                    chkMineForemanLaserRangeBoost.CheckState = CheckState.Indeterminate
                Case 1
                    chkMineForemanLaserRangeBoost.Checked = True
                Case 0
                    chkMineForemanLaserRangeBoost.Checked = False
            End Select

            ' Update the Booster boxes
            Call UpdateBoosterSkills()

            ' Refining
            chkMineRefinedOre.Checked = .RefinedOre
            chkMineUnrefinedOre.Checked = .UnrefinedOre
            chkMineCompressedOre.Checked = .CompressedOre

            Call SetOreRefineChecks()

            ' Station numbers
            cmbMineStationEff.Text = FormatPercent(.RefiningEfficiency, 0)
            txtMineRefineStanding.Text = FormatNumber(.RefineCorpStanding, 2)
            cmbMineRefineStationTax.Text = FormatPercent(.RefiningTax, 1)

            ' Jump Ore
            If .CheckIncludeJumpFuelCosts Then
                rbtnMineJumpCompress.Enabled = True
                rbtnMineJumpMinerals.Enabled = True
                lblMineTotalJumpFuel.Enabled = True
                txtMineTotalJumpFuel.Enabled = True
                lblMineTotalJumpM3.Enabled = True
                txtMineTotalJumpM3.Enabled = True
            Else
                rbtnMineJumpCompress.Enabled = False
                rbtnMineJumpMinerals.Enabled = False
                lblMineTotalJumpFuel.Enabled = False
                txtMineTotalJumpFuel.Enabled = False
                lblMineTotalJumpM3.Enabled = False
                txtMineTotalJumpM3.Enabled = False
            End If

            chkMineIncludeJumpCosts.Checked = .CheckIncludeJumpFuelCosts
            rbtnMineJumpCompress.Checked = .JumpCompressedOre
            rbtnMineJumpMinerals.Checked = .JumpMinerals
            txtMineTotalJumpFuel.Text = FormatNumber(.TotalJumpFuelCost, 2)
            txtMineTotalJumpM3.Text = FormatNumber(.TotalJumpFuelM3)

            ' Hauler
            If .CheckUseHauler Then
                lblMineHaulerM3.Enabled = False
                lblMineRTMin.Enabled = False
                lblMineRTSec.Enabled = False
            Else
                lblMineHaulerM3.Enabled = True
                lblMineRTMin.Enabled = True
                lblMineRTSec.Enabled = True
            End If

            chkMineUseHauler.Checked = .CheckUseHauler
            txtMineRTMin.Text = FormatNumber(.RoundTripMin, 0)
            txtMineRTSec.Text = FormatNumber(.RoundTripSec, 0)
            txtMineHaulerM3.Text = FormatNumber(.Haulerm3, 0)

            MiningColumnClicked = .ColumnSort
            If .ColumnSortType = "Ascending" Then
                MiningColumnSortType = SortOrder.Ascending
            Else
                MiningColumnSortType = SortOrder.Descending
            End If

            ' Taxes and Fees
            chkMineIncludeBrokerFees.Checked = .CheckIncludeFees
            chkMineIncludeTaxes.Checked = .CheckIncludeTaxes

            ' Michii
            chkMineMichiImplant.Checked = .MichiiImplant

            ' Number of miners
            txtMineNumberMiners.Text = CStr(.NumberofMiners)

            ' Upgrades and miner types - different for Ice or Ore
            If .OreType = "Ore" Then
                cmbMineShipType.Text = .OreMiningShip
                cmbMineMiningLaser.Text = .OreStrip
                If cmbMineMiningUpgrade.Items.Contains(.OreUpgrade) Then
                    cmbMineMiningUpgrade.Text = .OreUpgrade
                Else
                    cmbMineMiningUpgrade.Text = None
                End If
                cmbMineNumLasers.Text = CStr(.NumOreMiners)
                cmbMineNumMiningUpgrades.Text = CStr(.NumOreUpgrades)
                rbtnMineMercoxitRig.Enabled = True
                rbtnMineIceRig.Enabled = False
                If .MercoxitMiningRig Then
                    rbtnMineMercoxitRig.Checked = True
                Else
                    rbtnMineNoRigs.Checked = True
                End If
            ElseIf .OreType = "Ice" Then
                cmbMineShipType.Text = .IceMiningShip
                cmbMineMiningLaser.Text = .IceStrip
                If cmbMineMiningUpgrade.Items.Contains(.IceUpgrade) Then
                    cmbMineMiningUpgrade.Text = .IceUpgrade
                Else
                    cmbMineMiningUpgrade.Text = None
                End If
                cmbMineNumLasers.Text = CStr(.NumIceMiners)
                cmbMineNumMiningUpgrades.Text = CStr(.NumIceUpgrades)
                rbtnMineMercoxitRig.Enabled = False
                rbtnMineIceRig.Enabled = True
                If .IceMiningRig Then
                    rbtnMineIceRig.Checked = True
                Else
                    rbtnMineIceRig.Checked = True
                End If
            ElseIf .OreType = "Gas" Then
                cmbMineShipType.Text = .GasMiningShip
                cmbMineMiningLaser.Text = .GasHarvester
                cmbMineMiningUpgrade.Text = .GasUpgrade
                cmbMineNumLasers.Text = CStr(.NumGasHarvesters)
                cmbMineNumMiningUpgrades.Text = CStr(.NumGasUpgrades)
                rbtnMineMercoxitRig.Enabled = False
                rbtnMineIceRig.Enabled = False
                rbtnMineNoRigs.Checked = True
            End If

            If .OreType = "Ore" Then
                gbMineCrystals.Enabled = True
                If .T2Crystals Then
                    rbtnMineT1Crystals.Checked = False
                    rbtnMineT2Crystals.Checked = True
                Else
                    rbtnMineT1Crystals.Checked = True
                    rbtnMineT2Crystals.Checked = False
                End If
            Else
                gbMineCrystals.Enabled = False
            End If

            ' Implants
            Call UpdateMiningImplants()

            ' Load the ore processing skills
            For i = 1 To MineProcessingCheckBoxes.Count - 1
                TempSkillLevel = SelectedCharacter.Skills.GetSkillLevel(SelectedCharacter.Skills.GetSkillTypeID(MineProcessingLabels(i).Text))
                If TempSkillLevel <> 0 Then
                    MineProcessingCombos(i).Text = CStr(TempSkillLevel)
                    MineProcessingCheckBoxes(i).Checked = True
                Else
                    MineProcessingCombos(i).Text = "0"
                    MineProcessingCheckBoxes(i).Checked = False
                End If
            Next

            ' Update the ore processing skills
            Call UpdateProcessingSkills()

        End With

        ' Load all the skills for the character
        Call LoadCharacterMiningSkills()

        ' Updates the mining ship form with correct boxes enabled and equipment, etc
        Call UpdateMiningShipForm(True)

        ' Load up the ship image
        Call LoadMiningshipImage()

        ' Clear the grid
        lstMineGrid.Items.Clear()

    End Sub

    ' Main grid function for loading ores to mine
    Public Sub LoadMiningGrid()
        Dim SQL As String
        Dim readerMine As SQLiteDataReader
        Dim readerOre As SQLiteDataReader
        Dim i As Integer
        Dim lstOreRow As ListViewItem

        Dim IceMining As Boolean
        Dim GasMining As Boolean

        Dim StationRefineEfficiency As Double
        Dim StationRefineTax As Double

        Dim ShipMiningYield As Double
        Dim BaseCycleTime As Double
        Dim CycleTime As Double
        Dim CrystalMiningYield As Double
        Dim OreList As New List(Of MiningOre)
        Dim TempOre As MiningOre = Nothing
        Dim CrystalType As String = "" ' For reference out of getting crystals

        Dim Orem3PerSecond As Double
        Dim OrePerSecond As Double

        ' Ice stuff
        Dim IceCylesPerHour As Integer
        Dim IceBlocksPerHour As Integer
        ' Ice Hauling
        Dim IceBlocksPerLoad As Integer

        Dim RefineryYield As Double ' For reference out of refining

        ' For hauler calcs
        Dim SecondstoFill As Double  ' How much time it took to fill the m3 value with ore
        Dim FillCycles As Double ' How many cycles it will take to fill the m3 value with ore in an hour
        Dim RTTimetoStationSeconds As Long = 0 ' Seconds to get back to station to drop off ore

        Dim HeavyWaterCost As Double = 0 ' Total it costs to run the Rorq in deployed mode

        ' Error checks
        If Not CheckMiningEntryData() Then
            Exit Sub
        End If

        If cmbMineOreType.Text = "Ice" Then
            IceMining = True
        Else
            IceMining = False
        End If

        If cmbMineOreType.Text = "Gas" Then
            GasMining = True
        Else
            GasMining = False
        End If

        ' Get the refining stuff
        StationRefineEfficiency = CDbl(cmbMineStationEff.Text.Substring(0, Len(cmbMineStationEff.Text) - 1)) / 100
        StationRefineTax = CDbl(cmbMineRefineStationTax.Text.Substring(0, Len(cmbMineRefineStationTax.Text) - 1))

        If StationRefineTax > 0 Then
            StationRefineTax = StationRefineTax / 100
        Else
            StationRefineTax = 0
        End If

        ' Refining
        Dim RefinedMaterials As New Materials
        Dim RefiningStation As New RefiningReprocessing(CInt(cmbMineRefining.Text),
                                                        CInt(cmbMineRefineryEff.Text),
                                                        SelectedCharacter.Skills.GetSkillLevel(12196),
                                                        UserApplicationSettings.RefiningImplantValue,
                                                        StationRefineEfficiency, StationRefineTax, CDbl(txtMineRefineStanding.Text))

        ' First determine what type of stuff we are mining
        SQL = "SELECT ORES.ORE_ID, ORE_NAME, ORE_VOLUME, UNITS_TO_REFINE "
        SQL = SQL & "FROM ORES, ORE_LOCATIONS "
        SQL = SQL & "WHERE ORES.ORE_ID = ORE_LOCATIONS.ORE_ID "
        SQL = SQL & "AND BELT_TYPE = '" & cmbMineOreType.Text & "' "

        ' See if we want High yield ores
        If IceMining Then
            SQL = SQL & "AND ORES.HIGH_YIELD_ORE = -1 "
        ElseIf GasMining Then
            SQL = SQL & "AND ORES.HIGH_YIELD_ORE = -2 "
        Else
            If chkMineIncludeHighYieldOre.Checked = False Then
                ' Only base ores
                SQL = SQL & "AND ORES.HIGH_YIELD_ORE = 0 "
            End If
        End If

        ' See where we want this for security
        SQL = SQL & "AND SYSTEM_SECURITY IN ("

        If chkMineIncludeHighSec.Checked = True Then
            SQL = SQL & "'High Sec',"
        End If
        If chkMineIncludeLowSec.Checked = True Then
            SQL = SQL & "'Low Sec',"
        End If
        If chkMineIncludeNullSec.Checked = True Then
            SQL = SQL & "'Null Sec',"
        End If

        ' If WH checked, then add the classes
        If chkMineWH.Checked = True Then
            If chkMineC1.Checked And chkMineC1.Enabled Then
                SQL = SQL & "'C1',"
            End If
            If chkMineC2.Checked And chkMineC2.Enabled Then
                SQL = SQL & "'C2',"
            End If
            If chkMineC3.Checked And chkMineC3.Enabled Then
                SQL = SQL & "'C3',"
            End If
            If chkMineC4.Checked And chkMineC4.Enabled Then
                SQL = SQL & "'C4',"
            End If
            If chkMineC5.Checked And chkMineC5.Enabled Then
                SQL = SQL & "'C5',"
            End If
            If chkMineC6.Checked And chkMineC6.Enabled Then
                SQL = SQL & "'C6',"
            End If
        End If

        SQL = SQL.Substring(0, Len(SQL) - 1) & ") "

        ' Now determine what space we are looking at
        SQL = SQL & "AND SPACE IN ("

        If chkMineAmarr.Checked = True Then
            SQL = SQL & "'Amarr',"
        End If
        If chkMineCaldari.Checked = True Then
            SQL = SQL & "'Caldari',"
        End If
        If chkMineGallente.Checked = True Then
            SQL = SQL & "'Gallente',"
        End If
        If chkMineMinmatar.Checked = True Then
            SQL = SQL & "'Minmatar',"
        End If
        If chkMineWH.Checked = True Then
            SQL = SQL & "'WH',"
        End If

        SQL = SQL.Substring(0, Len(SQL) - 1) & ") "

        ' Group them
        SQL = SQL & "GROUP BY ORES.ORE_ID, ORE_NAME, ORE_VOLUME, UNITS_TO_REFINE "

        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerMine = DBCommand.ExecuteReader

        ' Process 
        ' Calculate the mining amount from all the stuff chosen except crystals

        ' Calculate cycle time for one laser

        ' Loop through ore, and determine the amount of ore the person can get per minute and include crystal amount
        ' because it's dependant on the ore type (and crystal type) and then we can look up skills on screen

        ' Adjust for hauling

        ' Then refine 60 minutes (ore per hour) of ore for each all in same loop

        ' For jumping amount - TBD

        ' Get the mining yield first (this is without crystals and no mercoxit)
        ShipMiningYield = CalculateMiningAmount()
        ' Duration is in milliseconds
        BaseCycleTime = CalculateMiningCycleTime(GetAttribute("duration", cmbMineMiningLaser.Text) / 1000)

        ' Get Heavy Water costs
        If (chkMineRorqDeployedMode.Checked Or chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate) And CInt(cmbMineIndustReconfig.Text) <> 0 Then
            ' Add (subtract from total isk) the heavy water cost
            HeavyWaterCost = CalculateRorqDeployedCost(CInt(cmbMineIndustReconfig.Text), CInt(cmbMineBoosterShipSkill.Text))
        End If

        lblMineCycleTime.Text = FormatNumber(BaseCycleTime, 1) & " s"

        Me.Cursor = Cursors.WaitCursor

        ' Loop through all the ores and determine ore amount, refine, 
        While readerMine.Read
            Application.DoEvents()
            ' DB Data
            TempOre.OreID = readerMine.GetInt64(0)
            TempOre.OreName = readerMine.GetString(1)
            TempOre.OreVolume = readerMine.GetDouble(2)
            TempOre.UnitsToRefine = readerMine.GetInt32(3)

            ' If not using a hauler, adjust the cycle time based on round trip time
            If chkMineUseHauler.Checked = False Then

                If TempOre.OreVolume > CDbl(txtMineHaulerM3.Text) Then
                    ' You can't use this hauler for this amount of ore
                    ' So no point in going on
                    MsgBox("The volume of the hauler is too small to use for this setup.", vbExclamation, Application.ProductName)
                    txtMineHaulerM3.Focus()
                    Exit Sub
                End If

                ' Get Round Trip Time (RTT) to station to drop off ore - User entered, in seconds
                RTTimetoStationSeconds = (CInt(txtMineRTMin.Text) * 60) + CInt(txtMineRTSec.Text)

            End If

            ' Ore amount
            If Not IceMining And Not GasMining Then
                ' Determine crystal amount
                CrystalMiningYield = ShipMiningYield * GetMiningCrystalBonus(TempOre.OreName, CrystalType)
                ' Save the crystal type
                TempOre.CrystalType = CrystalType
                TempOre.OreUnitsPerCycle = CrystalMiningYield

                ' Calculate the m3 per second for this ore including mining drone input
                Orem3PerSecond = (CrystalMiningYield / BaseCycleTime) + (CDbl(txtMineMiningDroneM3.Text) / 3600)

                ' This is the m3 per second, but need to get this ORE per second based on it's volume
                OrePerSecond = Orem3PerSecond / TempOre.OreVolume

            ElseIf IceMining Then
                TempOre.CrystalType = None
                IceCylesPerHour = CInt(Math.Floor(3600 / BaseCycleTime))

                ' Total ice blocks per hour
                IceBlocksPerHour = CInt(IceCylesPerHour * ShipMiningYield)
                ' Total ice blocks per cycle
                TempOre.OreUnitsPerCycle = ShipMiningYield * 1000 ' Ice is 1000 m3

            ElseIf GasMining Then
                ' Save the crystal type
                TempOre.CrystalType = None

                TempOre.OreUnitsPerCycle = ShipMiningYield
                Orem3PerSecond = ShipMiningYield / BaseCycleTime

                ' This is the m3 per second, but need to get this ORE per second based on it's volume
                OrePerSecond = Orem3PerSecond / TempOre.OreVolume
            End If

            If chkMineUseHauler.Checked = False Then
                ' Treat Ore and Gas the same
                If Not IceMining Then
                    ' How long to fill the cargo?
                    SecondstoFill = CDbl(txtMineHaulerM3.Text) / Orem3PerSecond
                    ' How many cycles where in this session?
                    FillCycles = SecondstoFill / BaseCycleTime

                    ' Add on the round trip time and recalculate cycle time
                    CycleTime = ((FillCycles * BaseCycleTime) + RTTimetoStationSeconds) / FillCycles

                    ' Recalculate with new cycle time
                    If cmbMineOreType.Text = "Ore" Then
                        Orem3PerSecond = (CrystalMiningYield / CycleTime) + (CDbl(txtMineMiningDroneM3.Text) / 3600)
                    Else ' Gas
                        Orem3PerSecond = ShipMiningYield / CycleTime
                    End If

                    ' This is the m3 per second, but need to get CycleTime ORE per second based on it's volume
                    OrePerSecond = Orem3PerSecond / TempOre.OreVolume

                Else ' Ice
                    ' How much can fit in cargo?
                    IceBlocksPerLoad = CInt(Math.Floor(CDbl(txtMineHaulerM3.Text) / TempOre.OreVolume))

                    ' How many full cycles to fill the cargo?
                    FillCycles = CInt(Math.Ceiling(IceBlocksPerLoad / ShipMiningYield))

                    ' Add on the round trip time and recalculate cycle time
                    CycleTime = ((FillCycles * BaseCycleTime) + RTTimetoStationSeconds) / FillCycles

                    ' Recalculate with new cycle time
                    IceCylesPerHour = CInt(Math.Floor(3600 / CycleTime))

                    ' Total ice blocks per hour
                    IceBlocksPerHour = CInt(IceCylesPerHour * ShipMiningYield)
                End If
            End If

            If IceMining Then
                TempOre.UnitsPerHour = IceBlocksPerHour
            Else
                TempOre.UnitsPerHour = OrePerSecond * 3600
            End If

            ' Only refine ore or ice
            If chkMineRefinedOre.Checked And Not GasMining Then
                ' Refine total Ore we mined for an hour and save the total isk/hour
                RefinedMaterials = RefiningStation.RefineOre(TempOre.OreID, GetOreProcessingSkill(TempOre.OreName), TempOre.UnitsPerHour,
                                                             chkMineIncludeTaxes.Checked, chkMineIncludeBrokerFees.Checked, RefineryYield)

                TempOre.RefineYield = RefineryYield

                TempOre.IPH = RefinedMaterials.GetTotalMaterialsCost - GetJumpCosts(RefinedMaterials, TempOre, TempOre.UnitsPerHour)
                If (chkMineRorqDeployedMode.Checked Or chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate) And CInt(cmbMineIndustReconfig.Text) <> 0 Then
                    ' Add (subtract from total isk) the heavy water cost
                    TempOre.IPH = TempOre.IPH - HeavyWaterCost
                End If

                ' Calculate the unit price by refining one batch
                RefinedMaterials = RefiningStation.RefineOre(TempOre.OreID, GetOreProcessingSkill(TempOre.OreName), TempOre.UnitsToRefine,
                                                             chkMineIncludeTaxes.Checked, chkMineIncludeBrokerFees.Checked, RefineryYield)
                TempOre.OreUnitPrice = RefinedMaterials.GetTotalMaterialsCost / TempOre.UnitsToRefine
                TempOre.RefineType = "Refined"
                OreList.Add(TempOre)

            End If

            If chkMineCompressedOre.Checked And Not GasMining Then
                ' First, get the unit price and volume for the compressed ore
                SQL = "SELECT PRICE FROM ITEM_PRICES WHERE ITEM_NAME LIKE 'Compressed " & TempOre.OreName & "'"
                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerOre = DBCommand.ExecuteReader

                If readerOre.Read() Then
                    TempOre.OreUnitPrice = readerOre.GetDouble(0)
                    ' Reset the units mined
                    Dim SavedUnits As Double = TempOre.UnitsPerHour
                    If Not IceMining Then
                        ' All ores are 100 to 1 compressed block, ice is 1 to 1
                        TempOre.UnitsPerHour = TempOre.UnitsPerHour / 100
                    End If

                    ' Units we mined, times unit price is IPH (minus Jump fuel costs)
                    TempOre.IPH = (TempOre.UnitsPerHour * TempOre.OreUnitPrice) - GetJumpCosts(Nothing, TempOre, SavedUnits) ' Treat the compression for jump costs individually, so use original value
                    TempOre.RefineYield = 0
                    TempOre.RefineType = "Compressed"
                    OreList.Add(TempOre)
                    ' Reset the units if we do unrefined
                    TempOre.UnitsPerHour = SavedUnits
                End If

                readerOre.Close()

            End If

            If chkMineUnrefinedOre.Checked Or GasMining Then ' Just use the Ore prices since we are selling it straight
                ' First, get the unit price for the ore
                SQL = "SELECT PRICE FROM ITEM_PRICES WHERE ITEM_NAME = '" & TempOre.OreName & "'"
                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerOre = DBCommand.ExecuteReader

                If readerOre.Read() Then
                    TempOre.OreUnitPrice = readerOre.GetDouble(0)
                    ' Units we mined, times unit price is IPH (minus Jump fuel costs)
                    TempOre.IPH = (TempOre.UnitsPerHour * TempOre.OreUnitPrice) - GetJumpCosts(Nothing, TempOre, TempOre.UnitsPerHour)

                    TempOre.RefineYield = 0
                    TempOre.RefineType = "Unrefined"
                    OreList.Add(TempOre)
                End If

                readerOre.Close()

            End If

        End While

        ' Sort the ore List by the iph
        OreList.Sort(New MiningOreIPHComparer)

        lstMineGrid.Items.Clear()

        ' Update column widths based on type - Ice, don't show Crystal, Gas, don't show Refine or Crystal
        Select Case cmbMineOreType.Text
            Case "Ore"
                lstMineGrid.Columns(1).Width = MineOreNameColumnWidth
                lstMineGrid.Columns(4).Width = MineRefineYieldColumnWidth
                lstMineGrid.Columns(5).Width = MineCrystalColumnWidth
            Case "Ice"
                lstMineGrid.Columns(1).Width = MineOreNameColumnWidth + MineCrystalColumnWidth
                lstMineGrid.Columns(4).Width = MineRefineYieldColumnWidth
                lstMineGrid.Columns(5).Width = 0 ' Hide
            Case "Gas"
                lstMineGrid.Columns(1).Width = MineOreNameColumnWidth + MineCrystalColumnWidth + MineRefineYieldColumnWidth
                lstMineGrid.Columns(4).Width = 0
                lstMineGrid.Columns(5).Width = 0 ' Hide
        End Select

        ' Determine multiplier - assume all additional mining ships have the same yield and other costs
        Dim MinerMultiplier As Integer = CInt(txtMineNumberMiners.Text)

        ' Finally load the list
        lstMineGrid.BeginUpdate()
        For i = 0 To OreList.Count - 1
            ' Make sure we want to add Mercoxit
            If Not OreList(i).OreName.Contains("Mercoxit") Or (OreList(i).OreName.Contains("Mercoxit") And cmbMineMiningLaser.Text.Contains("Deep Core")) Then
                lstOreRow = New ListViewItem(CStr(OreList(i).OreID))
                'The remaining columns are subitems  
                lstOreRow.SubItems.Add(OreList(i).OreName)
                lstOreRow.SubItems.Add(OreList(i).RefineType)
                lstOreRow.SubItems.Add(FormatNumber(OreList(i).OreUnitPrice, 2))
                If OreList(i).RefineYield = 0 Then
                    lstOreRow.SubItems.Add("-")
                Else
                    lstOreRow.SubItems.Add(FormatPercent(OreList(i).RefineYield, 3))
                End If
                lstOreRow.SubItems.Add(OreList(i).CrystalType)
                ' Modify all three by mining multiplier
                lstOreRow.SubItems.Add(FormatNumber(OreList(i).OreUnitsPerCycle * MinerMultiplier, 2))
                lstOreRow.SubItems.Add(FormatNumber(Math.Round(OreList(i).UnitsPerHour * MinerMultiplier), 0))
                lstOreRow.SubItems.Add(FormatNumber(OreList(i).IPH * MinerMultiplier, 2))
                Call lstMineGrid.Items.Add(lstOreRow)
            End If
        Next

        ' Now sort this
        Dim TempType As SortOrder
        If MiningColumnSortType = SortOrder.Ascending Then
            TempType = SortOrder.Descending
        Else
            TempType = SortOrder.Ascending
        End If
        Call ListViewColumnSorter(MiningColumnClicked, CType(lstMineGrid, ListView), MiningColumnClicked, TempType)
        Me.Cursor = Cursors.Default

        lstMineGrid.EndUpdate()

        ' Last thing, calculate the mining range of the mining lasers selected
        lblMineRange.Text = FormatNumber(CalculateMiningRange(GetAttribute("Optimal Range", cmbMineMiningLaser.Text)) / 1000, 2) & " km"

        i = 0
        Me.Cursor = Cursors.Default

    End Sub

    Public Sub LoadCharacterMiningSkills()

        ' Load the Mining Skills for this character
        cmbMineDeepCore.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(DeepCoreMiningSkillTypeID))
        cmbMineAstrogeology.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(AstrogeologySkillTypeID))
        cmbMineSkill.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(MiningSkillTypeID))
        cmbMineRefineryEff.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(ReprocessingEfficiencySkillTypeID))
        cmbMineRefining.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(ReprocessingSkillTypeID))

        ' If this is a dummy account, set these all to 1 - TODO Remove, or re-check so they work even if 0
        If cmbMineAstrogeology.Text = "" Then
            cmbMineAstrogeology.Text = "1"
        End If
        If cmbMineSkill.Text = "" Then
            cmbMineSkill.Text = "1"
        End If


        If cmbMineOreType.Text = "Gas" Then
            If SelectedCharacter.Skills.GetSkillLevel(GasCloudHarvestingSkillTypeID) = 0 Then
                ' Set it to base 1 - even though if they don't have this skill they can't fit a gas harvester
                cmbMineGasIceHarvesting.Text = "1"
            Else
                cmbMineGasIceHarvesting.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(GasCloudHarvestingSkillTypeID))
            End If
        ElseIf cmbMineOreType.Text = "Ice" Then
            cmbMineGasIceHarvesting.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(IceHarvestingSkillTypeID))
        Else
            cmbMineGasIceHarvesting.Text = "0"
        End If

        If cmbMineOreType.Text <> "Gas" Then
            cmbMineExhumers.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(ExhumersSkillTypeID))
        Else
            ' Load the Expedition frigate skill for the prospect
            cmbMineExhumers.Text = CStr(SelectedCharacter.Skills.GetSkillLevel(ExpeditionFrigatesSkillTypeID))
        End If

        Dim MiningBarge As Integer = SelectedCharacter.Skills.GetSkillLevel(MiningBargeSkillTypeID)
        Dim MiningFrigate As Integer = SelectedCharacter.Skills.GetSkillLevel(MiningFrigateSkillTypeID)

        If MiningBarge = 0 Then
            ' Look up Mining Frigate skill
            If MiningFrigate = 0 Then
                ' Just set it to 1
                cmbMineBaseShipSkill.Text = "1"
            Else
                cmbMineBaseShipSkill.Text = CStr(MiningFrigate)
            End If
        Else
            cmbMineBaseShipSkill.Text = CStr(MiningBarge)
        End If

    End Sub

    ' Saves all the settings on the screen selected
    Private Sub btnMineSaveAllSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMineSaveAllSettings.Click
        Dim TempSettings As MiningTabSettings = Nothing
        Dim Settings As New ProgramSettings

        ' Check data first
        If Not CheckMiningEntryData() Then
            Exit Sub
        End If

        With TempSettings
            ' Ore types
            .OreType = cmbMineOreType.Text

            .CheckHighSecOres = chkMineIncludeHighSec.Checked
            .CheckLowSecOres = chkMineIncludeLowSec.Checked
            .CheckNullSecOres = chkMineIncludeNullSec.Checked
            .CheckHighYieldOres = chkMineIncludeHighYieldOre.Checked

            .MiningDroneM3perHour = CDbl(txtMineMiningDroneM3.Text)
            .RefinedOre = chkMineRefinedOre.Checked
            .UnrefinedOre = chkMineUnrefinedOre.Checked
            .CompressedOre = chkMineCompressedOre.Checked

            ' Upgrades and miner types - different for Ice, Ore, or Gas
            If .OreType = "Ore" Then
                .OreMiningShip = cmbMineShipType.Text
                .OreStrip = cmbMineMiningLaser.Text
                .OreUpgrade = cmbMineMiningUpgrade.Text
                .NumOreMiners = CInt(cmbMineNumLasers.Text)
                .NumOreUpgrades = CInt(cmbMineNumMiningUpgrades.Text)
                .OreImplant = cmbMineImplant.Text

                ' Save the rigs
                If rbtnMineMercoxitRig.Checked = True Then
                    .MercoxitMiningRig = True
                Else
                    .MercoxitMiningRig = False
                End If

                ' Save Ice data
                .IceMiningShip = UserMiningTabSettings.IceMiningShip
                .IceStrip = UserMiningTabSettings.IceStrip
                .IceUpgrade = UserMiningTabSettings.IceUpgrade
                .NumIceMiners = UserMiningTabSettings.NumOreMiners
                .NumIceUpgrades = UserMiningTabSettings.NumOreUpgrades
                .IceImplant = UserMiningTabSettings.IceImplant
                .IceMiningRig = UserMiningTabSettings.IceMiningRig

                ' Save Gas Data
                .GasMiningShip = UserMiningTabSettings.GasMiningShip
                .GasHarvester = UserMiningTabSettings.GasHarvester
                .GasUpgrade = UserMiningTabSettings.GasUpgrade
                .NumGasHarvesters = UserMiningTabSettings.NumGasHarvesters
                .NumGasUpgrades = UserMiningTabSettings.NumGasUpgrades
                .GasImplant = UserMiningTabSettings.GasImplant

            ElseIf .OreType = "Ice" Then
                .IceMiningShip = cmbMineShipType.Text
                .IceStrip = cmbMineMiningLaser.Text
                .IceUpgrade = cmbMineMiningUpgrade.Text
                .NumIceMiners = CInt(cmbMineNumLasers.Text)
                .NumIceUpgrades = CInt(cmbMineNumMiningUpgrades.Text)
                .IceImplant = cmbMineImplant.Text

                ' Save Rig
                If rbtnMineIceRig.Checked = True Then
                    .IceMiningRig = True
                Else
                    .IceMiningRig = False
                End If

                ' Save Ore data
                .OreMiningShip = UserMiningTabSettings.OreMiningShip
                .OreStrip = UserMiningTabSettings.OreStrip
                .OreUpgrade = UserMiningTabSettings.OreUpgrade
                .NumOreMiners = UserMiningTabSettings.NumOreMiners
                .NumOreUpgrades = UserMiningTabSettings.NumOreUpgrades
                .OreImplant = UserMiningTabSettings.OreImplant
                .MercoxitMiningRig = UserMiningTabSettings.MercoxitMiningRig

                ' Save Gas Data
                .GasMiningShip = UserMiningTabSettings.GasMiningShip
                .GasHarvester = UserMiningTabSettings.GasHarvester
                .GasUpgrade = UserMiningTabSettings.GasUpgrade
                .NumGasHarvesters = UserMiningTabSettings.NumGasHarvesters
                .NumGasUpgrades = UserMiningTabSettings.NumGasUpgrades
                .GasImplant = UserMiningTabSettings.GasImplant

            ElseIf .OreType = "Gas" Then
                .GasMiningShip = cmbMineShipType.Text
                .GasHarvester = cmbMineMiningLaser.Text
                .GasUpgrade = None
                .NumGasHarvesters = CInt(cmbMineNumLasers.Text)
                .NumGasUpgrades = 0
                .GasImplant = cmbMineImplant.Text

                ' Save Ore data
                .OreMiningShip = UserMiningTabSettings.OreMiningShip
                .OreStrip = UserMiningTabSettings.OreStrip
                .OreUpgrade = UserMiningTabSettings.OreUpgrade
                .NumOreMiners = UserMiningTabSettings.NumOreMiners
                .NumOreUpgrades = UserMiningTabSettings.NumOreUpgrades
                .OreImplant = UserMiningTabSettings.OreImplant
                .MercoxitMiningRig = UserMiningTabSettings.MercoxitMiningRig

                ' Save Ice data
                .IceMiningShip = UserMiningTabSettings.IceMiningShip
                .IceStrip = UserMiningTabSettings.IceStrip
                .IceUpgrade = UserMiningTabSettings.IceUpgrade
                .NumIceMiners = UserMiningTabSettings.NumOreMiners
                .NumIceUpgrades = UserMiningTabSettings.NumOreUpgrades
                .IceImplant = UserMiningTabSettings.IceImplant
                .IceMiningRig = UserMiningTabSettings.IceMiningRig

            End If

            .T2Crystals = rbtnMineT2Crystals.Checked

            ' Fleet booster
            .CheckUseFleetBooster = chkMineUseFleetBooster.Checked
            .BoosterShip = cmbMineBoosterShip.Text
            .MiningDirectorSkill = CInt(cmbMineMiningDirector.Text)
            .MiningFormanSkill = CInt(cmbMineMiningForeman.Text)
            .BoosterShipSkill = CInt(cmbMineBoosterShipSkill.Text)
            .WarfareLinkSpecSkill = CInt(cmbMineWarfareLinkSpec.Text)
            .CheckMiningForemanMindLink = chkMineForemanMindlink.Checked
            .IndustrialReconfig = CInt(cmbMineIndustReconfig.Text)

            If chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate Then
                .CheckRorqDeployed = 2
            ElseIf chkMineRorqDeployedMode.Checked = True Then
                .CheckRorqDeployed = 1
            Else
                .CheckRorqDeployed = 0
            End If

            If chkMineForemanLaserOpBoost.CheckState = CheckState.Indeterminate Then
                .CheckMineForemanLaserOpBoost = 2
            ElseIf chkMineForemanLaserOpBoost.Checked = True Then
                .CheckMineForemanLaserOpBoost = 1
            Else
                .CheckMineForemanLaserOpBoost = 0
            End If

            If chkMineForemanLaserRangeBoost.CheckState = CheckState.Indeterminate Then
                .CheckMineForemanLaserRangeBoost = 2
            ElseIf chkMineForemanLaserRangeBoost.Checked = True Then
                .CheckMineForemanLaserRangeBoost = 1
            Else
                .CheckMineForemanLaserRangeBoost = 0
            End If

            ' Check all locations
            .CheckSovAmarr = chkMineAmarr.Checked
            .CheckSovCaldari = chkMineCaldari.Checked
            .CheckSovGallente = chkMineGallente.Checked
            .CheckSovMinmatar = chkMineMinmatar.Checked
            .CheckSovWormhole = chkMineWH.Checked

            .CheckSovC1 = chkMineC1.Checked
            .CheckSovC2 = chkMineC2.Checked
            .CheckSovC3 = chkMineC3.Checked
            .CheckSovC4 = chkMineC4.Checked
            .CheckSovC5 = chkMineC5.Checked
            .CheckSovC6 = chkMineC6.Checked

            ' Refining
            ' Station numbers
            If cmbMineStationEff.Text.Contains("%") Then
                .RefiningEfficiency = CDbl(cmbMineStationEff.Text.Substring(0, Len(cmbMineStationEff.Text) - 1)) / 100
            Else
                .RefiningEfficiency = CDbl(cmbMineStationEff.Text) / 100
            End If
            If cmbMineRefineStationTax.Text.Contains("%") Then
                .RefiningTax = CDbl(cmbMineRefineStationTax.Text.Substring(0, Len(cmbMineRefineStationTax.Text) - 1)) / 100
            Else
                .RefiningTax = CDbl(cmbMineRefineStationTax.Text) / 100
            End If

            ' Allow them to update the refine standing here as well
            .RefineCorpStanding = CDbl(txtMineRefineStanding.Text)

            ' Save it in the Application settings
            Settings.SaveApplicationSettings(UserApplicationSettings)

            ' Jump costs
            .CheckIncludeJumpFuelCosts = chkMineIncludeJumpCosts.Checked
            .JumpCompressedOre = rbtnMineJumpCompress.Checked
            .JumpMinerals = rbtnMineJumpMinerals.Checked
            .TotalJumpFuelCost = CDbl(txtMineTotalJumpFuel.Text)
            .TotalJumpFuelM3 = CDbl(txtMineTotalJumpM3.Text)

            .ColumnSort = MiningColumnClicked
            If MiningColumnSortType = SortOrder.Ascending Then
                .ColumnSortType = "Ascending"
            Else
                .ColumnSortType = "Decending"
            End If

            .CheckUseHauler = chkMineUseHauler.Checked

            ' Hauler - only save values if not using hauler
            If chkMineUseHauler.Checked = False Then
                .RoundTripMin = CInt(txtMineRTMin.Text)
                .RoundTripSec = CInt(txtMineRTSec.Text)
                .Haulerm3 = CDbl(txtMineHaulerM3.Text)
            Else
                .RoundTripMin = Settings.DefaultMiningRoundTripMin
                .RoundTripSec = Settings.DefaultMiningRoundTripSec
                .Haulerm3 = Settings.DefaultMiningHaulerm3
            End If

            ' Taxes and Fees
            .CheckIncludeFees = chkMineIncludeBrokerFees.Checked
            .CheckIncludeTaxes = chkMineIncludeTaxes.Checked

            ' Michii
            .MichiiImplant = chkMineMichiImplant.Checked

            ' Number of miners
            .NumberofMiners = CInt(txtMineNumberMiners.Text)

        End With

        ' Save the data in the XML file
        Call Settings.SaveMiningSettings(TempSettings)

        ' Save the data to the local variable
        UserMiningTabSettings = TempSettings

        MsgBox("Settings Saved", vbInformation, Application.ProductName)

    End Sub

    ' Sets the screen settings for ore type selected
    Private Sub SetOreRefineChecks()
        If cmbMineOreType.Text <> "Gas" Then
            If chkMineRefinedOre.Checked Then
                gbMineBaseRefineSkills.Enabled = True
                gbMineStationYield.Enabled = True
            Else
                gbMineBaseRefineSkills.Enabled = False
                gbMineStationYield.Enabled = False
            End If
            gbMineRefining.Enabled = True
        Else
            gbMineBaseRefineSkills.Enabled = False
            gbMineStationYield.Enabled = False
            gbMineRefining.Enabled = False
            If cmbMineOreType.Text = "Gas" Then
                ' Can't refine gas
                chkMineRefinedOre.Checked = False
                chkMineUnrefinedOre.Checked = False
                chkMineCompressedOre.Checked = False
            End If
        End If
    End Sub

    ' Loads the Fleet Boost Ship image
    Private Sub LoadFleetBoosterImage()
        If chkMineUseFleetBooster.Checked Then
            Dim ShipName As String

            If cmbMineBoosterShip.Text = "Other" Then
                ShipName = Rokh
            ElseIf cmbMineBoosterShip.Text = "Battlecruiser" Then
                ShipName = Drake
            Else
                ShipName = cmbMineBoosterShip.Text
            End If

            Dim BPImage As String = GetMiningShipImage(ShipName)

            If File.Exists(BPImage) Then
                pictMineFleetBoostShip.Image = Image.FromFile(BPImage)
            Else
                pictMineFleetBoostShip.Image = Nothing
            End If

        Else
            pictMineFleetBoostShip.Image = Nothing
        End If

        pictMineFleetBoostShip.Update()

    End Sub

    ' Loads the Mining Ship Image
    Private Sub LoadMiningshipImage()
        Dim ShipName As String

        If cmbMineShipType.Text = "Other" Then
            ShipName = Rokh
        ElseIf cmbMineShipType.Text = "Battlecruiser" Then
            ShipName = Drake
        Else
            ShipName = cmbMineShipType.Text
        End If

        Dim BPImage As String = GetMiningShipImage(ShipName)

        If File.Exists(BPImage) Then
            pictMineSelectedShip.Image = Image.FromFile(BPImage)
        Else
            pictMineSelectedShip.Image = Nothing
        End If

        pictMineSelectedShip.Update()

    End Sub

    ' Loads the implants for the mining type
    Private Sub UpdateMiningImplants()
        Dim ReqSkill As Integer

        ' Clear implants
        cmbMineImplant.Items.Clear()

        ' Set Ore or Ice implants
        If cmbMineOreType.Text = "Ice" Then
            cmbMineImplant.Items.Add(None)
            'Inherent Implants 'Yeti' Ice Harvesting IH-1001
            cmbMineImplant.Items.Add("'Yeti' IH-1001")
            cmbMineImplant.Items.Add("'Yeti' IH-1003")
            cmbMineImplant.Items.Add("'Yeti' IH-1005")

            ' No Michi for ice
            chkMineMichiImplant.Enabled = False
            chkMineMichiImplant.ForeColor = Color.Black

            cmbMineImplant.Text = UserMiningTabSettings.IceImplant

        ElseIf cmbMineOreType.Text = "Ore" Then
            'Inherent Implants 'Highwall' Mining MX-1001
            cmbMineImplant.Items.Add(None)
            cmbMineImplant.Items.Add("'Highwall' MX-1001")
            cmbMineImplant.Items.Add("'Highwall' MX-1003")
            cmbMineImplant.Items.Add("'Highwall' MX-1005")

            chkMineMichiImplant.Enabled = True

            ' Michi Implant
            ReqSkill = CInt(GetAttribute("requiredSkill1Level", "Michi's Excavation Augmentor"))
            If ReqSkill <> SelectedCharacter.Skills.GetSkillLevel(3411) Then
                chkMineMichiImplant.ForeColor = Color.Red
                If UserApplicationSettings.ShowToolTips Then
                    ttBP.SetToolTip(chkMineMichiImplant, "Requires Cybernetics " & ReqSkill)
                End If
            Else
                chkMineMichiImplant.ForeColor = Color.Black
            End If

            cmbMineImplant.Text = UserMiningTabSettings.OreImplant

        ElseIf cmbMineOreType.Text = "Gas" Then
            cmbMineImplant.Items.Add(None)
            'Eifyr and Co. 'Alchemist' Gas Harvesting GH-801
            cmbMineImplant.Items.Add("'Alchemist' GH-801")
            cmbMineImplant.Items.Add("'Alchemist' GH-803")
            cmbMineImplant.Items.Add("'Alchemist' GH-805")

            ' No Michi for gas
            chkMineMichiImplant.Enabled = False
            chkMineMichiImplant.ForeColor = Color.Black

            cmbMineImplant.Text = UserMiningTabSettings.GasImplant
        End If

    End Sub

    ' Updates the skills and combos associated with the ship selected
    Private Sub UpdateMiningShipForm(UpdateEquipment As Boolean)

        ' Update the mining skills first, ships loaded depend on these
        Call UpdateMiningSkills()

        ' Load the ships into the ship combo
        Call UpdateMiningShipsCombo()

        If UpdateEquipment Then
            ' Finally load all the ship equipment
            Call UpdateMiningShipEquipment()
        End If

    End Sub

    ' Updates the mining skills for the ships and equipment
    Private Sub UpdateMiningSkills()
        ' Mining upgrades - need mining upgrades 1 or 4 for T2
        ' * mercoxit - T2's Deep core Mining 2
        ' Deep Core Strip Mining skill - Astrogeology 5 and Mining 5
        ' Ice miners - Need to change the mining combo name to Ice Harvesting
        ' * Need level 4 for T1 and level 5 for T2
        ' If they choose 'Other' for ship, hide strip miners and then show 'Miners' and number
        ' If they choose frigs or cruisers, show skill level of ship along with miners

        ' Mining Skill
        ' 3 for mining upgrades
        ' 4 for Astrology and ice harvesting
        ' 5 for Deep core mining

        If cmbMineSkill.Text = "" Then
            cmbMineSkill.Text = "1"
        End If

        ' Mining upgrades (ice and ore)
        If CInt(cmbMineSkill.Text) >= 3 And cmbMineOreType.Text <> "Gas" Then
            cmbMineMiningUpgrade.Enabled = True
        Else
            cmbMineMiningUpgrade.Enabled = False
        End If

        ' Ice/Gas Harvesting skill
        If CInt(cmbMineSkill.Text) >= 4 Then
            If cmbMineOreType.Text = "Ice" Then
                lblMineGasIceHarvesting.Text = "Ice Harv:"
                lblMineGasIceHarvesting.Enabled = True
                cmbMineGasIceHarvesting.Enabled = True
                cmbMineAstrogeology.Enabled = True
            ElseIf cmbMineOreType.Text = "Gas" Then
                lblMineGasIceHarvesting.Text = "Gas Harv:"
                lblMineGasIceHarvesting.Enabled = True
                cmbMineGasIceHarvesting.Enabled = True
                cmbMineAstrogeology.Enabled = False
            ElseIf cmbMineOreType.Text = "Ore" Then
                lblMineGasIceHarvesting.Enabled = False
                cmbMineGasIceHarvesting.Enabled = False
                cmbMineAstrogeology.Enabled = True
            End If
        Else
            cmbMineAstrogeology.Enabled = False
            lblMineGasIceHarvesting.Enabled = False
            cmbMineGasIceHarvesting.Enabled = False
        End If

        If cmbMineAstrogeology.Text = "" Then
            cmbMineAstrogeology.Text = "1"
        End If

        ' Deep core only for asteroid mining
        If CInt(cmbMineSkill.Text) = 5 And CInt(cmbMineAstrogeology.Text) = 5 And cmbMineOreType.Text = "Ore" Then
            cmbMineDeepCore.Enabled = True
            lblMineDeepCore.Enabled = True
        Else
            cmbMineDeepCore.Enabled = False
            lblMineDeepCore.Enabled = False
        End If

        ' Set exhumer skill combo for ice, but the prospect can mine ore so enable it for all
        If cmbMineOreType.Text = "Ice" Then
            If CInt(cmbMineAstrogeology.Text) = 5 And cmbMineAstrogeology.Enabled = True And CInt(cmbMineBaseShipSkill.Text) = 5 Then
                cmbMineExhumers.Enabled = True
                lblMineExhumers.Enabled = True
            Else
                cmbMineExhumers.Enabled = False
                lblMineExhumers.Enabled = False
            End If
        Else
            cmbMineExhumers.Enabled = True
            lblMineExhumers.Enabled = True
        End If

        ' Set true, can be set false when they choose "other"
        cmbMineBaseShipSkill.Enabled = True
        lblMineBaseShipSkill.Enabled = True

        ' Set the skill level of the ship they selected if not a mining barge/exhumer
        Select Case cmbMineShipType.Text
            Case "Other"
                cmbMineBaseShipSkill.Enabled = False
                lblMineBaseShipSkill.Enabled = False
                cmbMineExhumers.Enabled = False
                lblMineExhumers.Enabled = False
        End Select

    End Sub

    ' Updates the ships combo with ships based on the levels of skills set
    Private Sub UpdateMiningShipsCombo()
        Dim PreviousShip As String = cmbMineShipType.Text
        Dim MaxShipName As String = ""
        Dim ShipSkillLevel As Integer = 0

        UpdatingMiningShips = True
        cmbMineShipType.Items.Clear()

        If cmbMineBaseShipSkill.Text = "" Then
            cmbMineBaseShipSkill.Text = "1"
        End If

        If cmbMineExhumers.Text = "" Then
            cmbMineExhumers.Text = "1"
        End If

        ' For gas and ore, load venture, prospect and other
        If cmbMineOreType.Text <> "Ice" Then
            ' Check for Mining Frigate skill to load the Venture
            If CInt(cmbMineBaseShipSkill.Text) >= 1 Then
                cmbMineShipType.Items.Add(Venture)
                MaxShipName = Venture
            End If

            ' Use exhumers skill for expedition frigate
            If CInt(cmbMineExhumers.Text) >= 1 Then
                cmbMineShipType.Items.Add(Prospect)
                cmbMineShipType.Items.Add(Endurance)
                MaxShipName = Prospect
            End If

            ' Always add other for non ICE mining
            cmbMineShipType.Items.Add("Other")
        End If

        If cmbMineOreType.Text <> "Gas" Then
            ' Exhumers and Mining Barges - Load for both Ice and Ore
            ' 3 for Mining barge, Procurer, 4 for Retriever, 5 for Covetor
            ' 5 for Exhumers and Deep core mining
            'Covetor, Retriever, Procurer. Hulk, Skiff, Mackinaw
            If CInt(cmbMineAstrogeology.Text) >= 3 And cmbMineAstrogeology.Enabled = True And CInt(cmbMineBaseShipSkill.Text) >= 1 Then
                cmbMineShipType.Items.Add(Procurer)
                MaxShipName = Procurer
                cmbMineShipType.Items.Add(Retriever)
                MaxShipName = Retriever
                cmbMineShipType.Items.Add(Covetor)
                MaxShipName = Covetor
            End If

            If CInt(cmbMineAstrogeology.Text) = 5 And cmbMineAstrogeology.Enabled = True And CInt(cmbMineBaseShipSkill.Text) = 5 And CInt(cmbMineExhumers.Text) >= 1 Then
                cmbMineShipType.Items.Add(Skiff)
                MaxShipName = Skiff
                cmbMineShipType.Items.Add(Mackinaw)
                MaxShipName = Mackinaw
                cmbMineShipType.Items.Add(Hulk)
                MaxShipName = Hulk
            End If
        End If

        If cmbMineOreType.Text = "Ice" And CInt(cmbMineBaseShipSkill.Text) = 5 And CInt(cmbMineExhumers.Text) >= 1 Then
            ' Add the prospect and endurance
            cmbMineShipType.Items.Add(Venture)
            cmbMineShipType.Items.Add(Endurance)
            cmbMineShipType.Items.Add(Prospect)
            MaxShipName = Endurance
        End If

        If MaxShipName = "" And cmbMineOreType.Text <> "Ice" Then
            MaxShipName = "Other"
        ElseIf MaxShipName = "" And cmbMineOreType.Text = "Ice" Then ' Only 6 ships can mine ice.
            MaxShipName = None
            ' Always add None for this case
            cmbMineShipType.Items.Add(None)
        End If

        ' Use settings to load the ships, else load the maxshipname unless first load
        If cmbMineOreType.Text = "Ore" And UserMiningTabSettings.OreMiningShip <> "" And FirstShowMining Then
            cmbMineShipType.Text = UserMiningTabSettings.OreMiningShip
        ElseIf cmbMineOreType.Text = "Ice" And UserMiningTabSettings.IceMiningShip <> "" And FirstShowMining Then
            cmbMineShipType.Text = UserMiningTabSettings.IceMiningShip
        ElseIf cmbMineOreType.Text = "Gas" And UserMiningTabSettings.GasMiningShip <> "" And FirstShowMining Then
            cmbMineShipType.Text = UserMiningTabSettings.GasMiningShip
        Else
            If cmbMineShipType.Items.Contains(PreviousShip) Then
                cmbMineShipType.Text = PreviousShip
            Else
                cmbMineShipType.Text = MaxShipName
            End If
        End If

        ' If we have a max ship name, then set it if it didn't stick in the combo after checking settings
        If MaxShipName <> "" And cmbMineShipType.Text = "" Then
            cmbMineShipType.Text = MaxShipName
        End If

        UpdatingMiningShips = False
        Call LoadMiningshipImage()

    End Sub

    ' Loads the laser/strip combos, implant, etc for the ship types
    Private Sub UpdateMiningShipEquipment()
        Dim LaserCount As Integer = 0
        Dim MLUCount As Integer = 0
        Dim i As Integer
        Dim MaxStrip As String = ""
        Dim T1Module As String = ""
        Dim ShipName As String
        Dim DeepCoreLoaded As Boolean = False

        Dim SQL As String
        Dim rsMiners As SQLiteDataReader

        ' Load up the main mining laser query - set groupID for search in processing
        SQL = "SELECT typeName, CASE WHEN metaGroupID IS NULL THEN 1 ELSE metaGroupID END AS TECH "
        SQL &= "FROM INVENTORY_TYPES "
        SQL &= "LEFT JOIN META_TYPES ON INVENTORY_TYPES.typeID = META_TYPES.typeID "
        SQL &= "WHERE published <> 0 "

        ' Clear miners
        cmbMineMiningLaser.Items.Clear()

        ShipName = cmbMineShipType.Text

        Select Case ShipName
            Case Hulk, Mackinaw, Skiff, Covetor, Retriever, Procurer
                ' Get the numbers
                LaserCount = CInt(GetAttribute("High Slots", ShipName))
                MLUCount = CInt(GetAttribute("Low Slots", ShipName))

                ' Now load the strips
                If cmbMineOreType.Text = "Ore" Then
                    ' Mining Skill
                    ' Mining 4 for T1 Strips
                    ' Mining 5 for T2 Strips
                    SQL &= "AND INVENTORY_TYPES.groupID IN (464, 483) AND typeName NOT LIKE '%Ice%' "
                    If CInt(cmbMineSkill.Text) < 5 Then
                        SQL &= "AND TECH <> 2 AND typeName NOT LIKE '%Deep Core%' " ' Don't load the deep core or tech 2
                        rbtnMineT1Crystals.Enabled = False
                        rbtnMineT2Crystals.Enabled = False
                    ElseIf CInt(cmbMineSkill.Text) = 5 And cmbMineDeepCore.Enabled = False Then
                        SQL &= " AND typeName NOT LIKE '%Deep Core%' " ' Don't load the deep core
                        rbtnMineT1Crystals.Enabled = True
                        rbtnMineT2Crystals.Enabled = True
                    ElseIf CInt(cmbMineDeepCore.Text) >= 2 And cmbMineDeepCore.Enabled = True Then
                        ' Load them all
                        rbtnMineT1Crystals.Enabled = True
                        rbtnMineT2Crystals.Enabled = True
                    End If
                    SQL &= "ORDER BY typeName"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    rsMiners = DBCommand.ExecuteReader

                    While rsMiners.Read
                        cmbMineMiningLaser.Items.Add(rsMiners.GetString(0))
                        If rsMiners.GetInt32(1) = 1 Then
                            T1Module = rsMiners.GetString(0)
                        End If
                    End While

                    If cmbMineMiningLaser.Items.Contains(UserMiningTabSettings.OreStrip) Then
                        MaxStrip = UserMiningTabSettings.OreStrip
                    Else
                        MaxStrip = T1Module
                    End If

                Else
                    ' Ice harvesting skill
                    ' 1 for T1 strip
                    ' 5 for T2 strips
                    SQL &= "AND INVENTORY_TYPES.groupID IN (464, 483) AND typeName LIKE '%Ice%' "
                    If CInt(cmbMineGasIceHarvesting.Text) < 5 Then
                        SQL &= "AND TECH <> 2 " ' Don't load tech 2
                    End If
                    SQL &= "ORDER BY typeName"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    rsMiners = DBCommand.ExecuteReader

                    While rsMiners.Read
                        cmbMineMiningLaser.Items.Add(rsMiners.GetString(0))
                        If rsMiners.GetInt32(1) = 1 Then
                            T1Module = rsMiners.GetString(0)
                        End If
                    End While

                    rbtnMineT1Crystals.Enabled = False
                    rbtnMineT2Crystals.Enabled = False

                    If cmbMineMiningLaser.Items.Contains(UserMiningTabSettings.OreStrip) Then
                        MaxStrip = UserMiningTabSettings.OreStrip
                    Else
                        MaxStrip = T1Module
                    End If

                End If

                ' Turn on the num lasers
                lblMineLaserNumber.Enabled = True
                cmbMineNumLasers.Enabled = True

            Case Else ' Other ships that are not mining barges
                LaserCount = CInt(GetAttribute("Turret Hardpoints", ShipName)) ' Use turret hardpoints for this
                MLUCount = CInt(GetAttribute("Low Slots", ShipName))

                ' For Other Ships
                lblMineLaserNumber.Visible = True
                cmbMineNumLasers.Visible = True

                If cmbMineOreType.Text = "Ore" Then
                    rbtnMineT1Crystals.Enabled = False
                    rbtnMineT2Crystals.Enabled = False

                    ' Add all the basic mining lasers
                    SQL &= "AND (INVENTORY_TYPES.groupID = 54 OR (INVENTORY_TYPES.groupID = 483 AND typeName NOT LIKE '%Strip%')) AND typeName NOT LIKE '%Ice%' "

                    If CInt(cmbMineSkill.Text) < 4 Then
                        SQL &= "AND TECH <> 2 AND typeName NOT LIKE '%Deep Core%' " ' Don't load T2 or any others
                    ElseIf CInt(cmbMineSkill.Text) < 5 Then
                        SQL &= "AND typeName NOT LIKE '%Deep Core%' " ' No deep core if not 5
                    ElseIf cmbMineDeepCore.Enabled = False Or CInt(cmbMineDeepCore.Text) = 0 Then
                        SQL &= " AND typeName NOT LIKE '%Deep Core%'" ' Don't load the deep core if not enabled
                    ElseIf CInt(cmbMineDeepCore.Text) >= 1 And CInt(cmbMineDeepCore.Text) <= 2 And cmbMineDeepCore.Enabled = True Then
                        SQL &= " AND typeName NOT LIKE '%Modulated Deep Core%'" ' Don't load the modulated deep core
                    ElseIf CInt(cmbMineDeepCore.Text) >= 2 And cmbMineDeepCore.Enabled = True Then
                        ' Deep core is fine
                        rbtnMineT1Crystals.Enabled = True
                        rbtnMineT2Crystals.Enabled = True
                    End If
                    SQL &= "ORDER BY typeName"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    rsMiners = DBCommand.ExecuteReader

                    While rsMiners.Read
                        cmbMineMiningLaser.Items.Add(rsMiners.GetString(0))
                        If rsMiners.GetInt32(1) = 1 Then
                            T1Module = rsMiners.GetString(0)
                        End If
                    End While

                    If cmbMineMiningLaser.Items.Contains(UserMiningTabSettings.OreStrip) Then
                        MaxStrip = UserMiningTabSettings.OreStrip
                    Else
                        MaxStrip = T1Module
                    End If

                ElseIf cmbMineOreType.Text = "Gas" Then
                    ' Only venture and other ships
                    SQL &= "AND INVENTORY_TYPES.groupID = 737 " ' Gas harvesters
                    If CInt(cmbMineGasIceHarvesting.Text) < 5 Then
                        SQL &= "AND TECH <> 2 " ' Don't load the tech 2
                    End If
                    SQL &= "ORDER BY typeName"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    rsMiners = DBCommand.ExecuteReader

                    While rsMiners.Read
                        cmbMineMiningLaser.Items.Add(rsMiners.GetString(0))
                        If rsMiners.GetInt32(1) = 1 Then
                            T1Module = rsMiners.GetString(0)
                        End If
                    End While

                    If cmbMineMiningLaser.Items.Contains(UserMiningTabSettings.OreStrip) Then
                        MaxStrip = UserMiningTabSettings.OreStrip
                    Else
                        MaxStrip = T1Module
                    End If

                ElseIf cmbMineOreType.Text = "Ice" And (ShipName = Endurance Or ShipName = Prospect Or ShipName = Venture) Then
                    SQL &= "AND INVENTORY_TYPES.groupID = 54 AND typeName LIKE '%Ice%' "
                    If CInt(cmbMineGasIceHarvesting.Text) < 5 Then
                        SQL &= "AND TECH <> 2 " ' Don't load tech 2
                    End If
                    SQL &= "ORDER BY typeName"

                    DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                    rsMiners = DBCommand.ExecuteReader

                    While rsMiners.Read
                        cmbMineMiningLaser.Items.Add(rsMiners.GetString(0))
                        If rsMiners.GetInt32(1) = 1 Then
                            T1Module = rsMiners.GetString(0)
                        End If
                    End While

                    If cmbMineMiningLaser.Items.Contains(UserMiningTabSettings.OreStrip) Then
                        MaxStrip = UserMiningTabSettings.OreStrip
                    Else
                        MaxStrip = T1Module
                    End If

                End If

        End Select

        ' Set crystals
        rbtnMineT2Crystals.Checked = UserMiningTabSettings.T2Crystals

        ' Set Strip name
        cmbMineMiningLaser.Text = MaxStrip

        ' Set the MLU numbers
        cmbMineNumMiningUpgrades.Items.Clear()
        cmbMineNumMiningUpgrades.Enabled = True

        ' Allow 8 for MLU's if other ship (Rohk, Iteron, etc)
        If ShipName = "Other" And cmbMineOreType.Text <> "Gas" And cmbMineOreType.Text <> "Ice" Then
            MLUCount = 8
            LaserCount = 8
        ElseIf cmbMineOreType.Text = "Gas" Then
            MLUCount = 0
            ' Update laser count based on skills - max of 5 but no more than turrets 
            If cmbMineShipType.Text = Venture Or cmbMineShipType.Text = Prospect Or cmbMineShipType.Text = Endurance Then
                ' Update the laser count if it's less than the turrets on the venture/prospect
                If CInt(cmbMineGasIceHarvesting.Text) < LaserCount Then
                    LaserCount = CInt(cmbMineGasIceHarvesting.Text)
                End If
            Else ' Other Ship
                ' Update the turrets based on the skill, max of 5
                LaserCount = CInt(cmbMineGasIceHarvesting.Text)
            End If
        End If

        For i = 1 To MLUCount
            cmbMineNumMiningUpgrades.Items.Add(CStr(i))
        Next

        ' Set the number of Strip miners available
        cmbMineNumLasers.Items.Clear()

        ' Set the max allowable lasers
        For i = 1 To LaserCount
            cmbMineNumLasers.Items.Add(CStr(i))
        Next

        ' Choose options for None ships first, these should be just clear settings
        If cmbMineShipType.Text = None Then

            cmbMineNumMiningUpgrades.Text = "0"
            cmbMineMiningUpgrade.Text = None
            cmbMineNumLasers.Text = "0"
            cmbMineMiningLaser.Text = None

        Else ' Normal ship or "other"
            ' Load settings, only change to user settings if the ship is the same as the one selected

            ' Set the names and numbers for upgrades and strips
            If cmbMineOreType.Text = "Ore" Then
                ' Set the number of MLUs
                If UserMiningTabSettings.NumOreUpgrades = 0 Or MLUCount < UserMiningTabSettings.NumOreUpgrades Or ShipName <> UserMiningTabSettings.OreMiningShip Then
                    cmbMineNumMiningUpgrades.Text = CStr(MLUCount)
                Else
                    cmbMineNumMiningUpgrades.Text = CStr(UserMiningTabSettings.NumOreUpgrades)
                End If

                ' Set the MLU Text - These are hardcoded so just use the default or user setting
                cmbMineMiningUpgrade.Text = UserMiningTabSettings.OreUpgrade

                ' Set number of strips
                If UserMiningTabSettings.NumOreMiners = 0 Or LaserCount < UserMiningTabSettings.NumOreMiners Or ShipName <> UserMiningTabSettings.OreMiningShip Then
                    cmbMineNumLasers.Text = CStr(LaserCount)
                Else
                    cmbMineNumLasers.Text = CStr(UserMiningTabSettings.NumOreMiners)
                End If

                ' Set Strip name
                If UserMiningTabSettings.OreStrip = "" Then
                    cmbMineMiningLaser.Text = MaxStrip
                Else
                    cmbMineMiningLaser.Text = UserMiningTabSettings.OreStrip
                End If

            ElseIf cmbMineOreType.Text = "Ice" Then
                ' Set the number of MLUs
                If UserMiningTabSettings.NumIceUpgrades = 0 Or MLUCount < UserMiningTabSettings.NumIceUpgrades Or ShipName <> UserMiningTabSettings.IceMiningShip Then
                    cmbMineNumMiningUpgrades.Text = CStr(MLUCount)
                Else
                    cmbMineNumMiningUpgrades.Text = CStr(UserMiningTabSettings.NumIceUpgrades)
                End If

                ' Set the MLU Text - These are hardcoded so just use the default or user setting
                cmbMineMiningUpgrade.Text = UserMiningTabSettings.IceUpgrade

                ' Set number of strips
                If UserMiningTabSettings.NumIceMiners = 0 Or LaserCount < UserMiningTabSettings.NumIceMiners Or ShipName <> UserMiningTabSettings.IceMiningShip Then
                    cmbMineNumLasers.Text = CStr(LaserCount)
                Else
                    ' Update with the user settings they have, up to the max the ship can use
                    cmbMineNumLasers.Text = CStr(UserMiningTabSettings.NumIceMiners)
                End If

                ' Set Strip name
                If UserMiningTabSettings.IceStrip = "" Then
                    cmbMineMiningLaser.Text = MaxStrip
                Else
                    cmbMineMiningLaser.Text = UserMiningTabSettings.IceStrip
                End If

            ElseIf cmbMineOreType.Text = "Gas" Then
                ' No MLUs for gas
                cmbMineNumMiningUpgrades.Enabled = False
                cmbMineMiningUpgrade.Text = UserMiningTabSettings.GasUpgrade

                ' Set number of strips
                If UserMiningTabSettings.NumIceMiners = 0 Or MLUCount < UserMiningTabSettings.NumGasHarvesters Or ShipName <> UserMiningTabSettings.GasMiningShip Then
                    cmbMineNumLasers.Text = CStr(LaserCount)
                Else
                    cmbMineNumLasers.Text = CStr(UserMiningTabSettings.NumGasHarvesters)
                End If

                ' Set Strip name
                If UserMiningTabSettings.IceStrip = "" Then
                    cmbMineMiningLaser.Text = MaxStrip
                Else
                    cmbMineMiningLaser.Text = UserMiningTabSettings.GasHarvester
                End If
            End If

        End If

        Call RefreshHaulerM3()

        lblMineCycleTime.Text = ""
        lblMineRange.Text = ""

    End Sub

    ' Updates the processing skills (enable, disable) depending on the refining skills selected
    Private Sub UpdateProcessingSkills()

        If FirstLoad Then
            Exit Sub
        End If

        ' Set them all false first
        For i = 1 To MineProcessingCheckBoxes.Count - 1
            MineProcessingCheckBoxes(i).Enabled = False
        Next

        For i = 1 To MineProcessingCombos.Count - 1
            MineProcessingCombos(i).Enabled = False
        Next

        For i = 1 To MineProcessingLabels.Count - 1
            MineProcessingLabels(i).Enabled = False
        Next

        cmbMineRefineryEff.Enabled = False

        If cmbMineOreType.Text = "Ore" Then

            If cmbMineRefining.Text = "4" Or cmbMineRefining.Text = "5" Then
                ' Veld, Scordite, Pyroxeres, and Plag
                Call EnableOreProcessingGroup(1, True)
                Call EnableOreProcessingGroup(2, True)
                Call EnableOreProcessingGroup(9, True)
                Call EnableOreProcessingGroup(10, True)

                ' Reprocessing 4 is needed for this instead of 5
                cmbMineRefineryEff.Enabled = True
            End If

            If cmbMineRefining.Text = "5" Then
                ' Hemo, Jaspet, Kernite, Omber, Refinery Effy
                Call EnableOreProcessingGroup(3, True)
                Call EnableOreProcessingGroup(4, True)
                Call EnableOreProcessingGroup(11, True)
                Call EnableOreProcessingGroup(12, True)



            End If

            If cmbMineRefineryEff.Text = "4" Or cmbMineRefineryEff.Text = "5" Then
                ' Dark Ochre, Gneiss, Hedb, Spod
                Call EnableOreProcessingGroup(5, True)
                Call EnableOreProcessingGroup(6, True)
                Call EnableOreProcessingGroup(13, True)
                Call EnableOreProcessingGroup(14, True)
            End If

            If cmbMineRefineryEff.Text = "5" Then
                ' Ark, Bisot, Crokite, Mercoxit
                Call EnableOreProcessingGroup(7, True)
                Call EnableOreProcessingGroup(8, True)
                Call EnableOreProcessingGroup(15, True)
                Call EnableOreProcessingGroup(16, True)
            End If

        ElseIf cmbMineOreType.Text = "Ice" Then
            ' Enable the one ice processing skill
            If cmbMineRefining.Text = "5" Then
                cmbMineRefineryEff.Enabled = True
            End If

            If cmbMineRefineryEff.Enabled And cmbMineRefineryEff.Text = "5" Then
                Call EnableOreProcessingGroup(17, True)
            End If

        Else ' Gas
            ' We don't refine, so leave them all off
        End If

    End Sub

    ' Changes the ore processing skill group to enabled or disabled
    Private Sub EnableOreProcessingGroup(ByVal Index As Integer, ByVal EnableObject As Boolean)
        If MineProcessingCheckBoxes(Index).Checked And EnableObject Then
            ' Ok to enable
            MineProcessingCombos(Index).Enabled = True
            MineProcessingLabels(Index).Enabled = True
        Else
            ' Don't enable
            MineProcessingCombos(Index).Enabled = False
            MineProcessingLabels(Index).Enabled = False
        End If

        MineProcessingCheckBoxes(Index).Enabled = EnableObject
    End Sub

    ' Updates the skills and boxes associated with the booster
    Private Sub UpdateBoosterSkills()
        Dim CurrentShip As String

        ' Industrial command ships = Orca/Porpoise. Need Mining director 1
        ' Capital industrial = rorq, need nothing

        ' Mining director needs mining foreman 5
        ' Mindlink (implant) needs mining director 5 
        ' Mining Foreman Link 1 needs mining foreman 5
        ' Mining Foreman Link 2 needs mining director 1
        If chkMineUseFleetBooster.Checked Then
            cmbMineBoosterShip.Enabled = True
            cmbMineMiningForeman.Enabled = True
            lblMineBoosterShipSkill.Enabled = True
            cmbMineBoosterShipSkill.Enabled = True
            lblMineWarfareLinkSpec.Enabled = True
            cmbMineWarfareLinkSpec.Enabled = True

            If cmbMineMiningForeman.Text = "5" Then
                cmbMineMiningDirector.Enabled = True

                If cmbMineMiningDirector.Text >= "1" Then
                    chkMineForemanMindlink.Enabled = True ' Implant
                    chkMineForemanLaserOpBoost.ThreeState = True ' Allow for t2 mindlink
                    chkMineForemanLaserOpBoost.Enabled = True
                    chkMineForemanLaserRangeBoost.ThreeState = True ' Allow for t2 mindlink
                    chkMineForemanLaserRangeBoost.Enabled = True
                Else
                    chkMineForemanMindlink.Enabled = False
                    chkMineForemanLaserOpBoost.Enabled = True
                    chkMineForemanLaserOpBoost.ThreeState = False ' Only the T1 mindlink
                    chkMineForemanLaserRangeBoost.Enabled = True
                    chkMineForemanLaserRangeBoost.ThreeState = False ' Only the T1 mindlink
                End If
            Else
                chkMineForemanLaserOpBoost.Enabled = False
                chkMineForemanLaserRangeBoost.Enabled = False
                chkMineForemanMindlink.Enabled = False
                cmbMineMiningDirector.Enabled = False
            End If

            Call UpdateMiningBoosterObjects()

            UpdatingMiningShips = True

            CurrentShip = cmbMineBoosterShip.Text
            cmbMineBoosterShip.Items.Clear()

            cmbMineBoosterShip.Items.Add(Rorqual)
            cmbMineBoosterShip.Items.Add(Orca)
            cmbMineBoosterShip.Items.Add(Porpoise)
            cmbMineBoosterShip.Items.Add("Battlecruiser")
            cmbMineBoosterShip.Items.Add("Other")

            If cmbMineBoosterShip.Items.Contains(CurrentShip) Then
                cmbMineBoosterShip.Text = CurrentShip
            Else
                cmbMineBoosterShip.Text = "Other"
            End If

            UpdatingMiningShips = False

            If cmbMineBoosterShip.Text = "Other" Then
                ' Disable mining foreman link
                chkMineForemanLaserOpBoost.Enabled = False
                chkMineForemanLaserRangeBoost.Enabled = False
            End If

            If cmbMineBoosterShip.Text = Orca Or cmbMineBoosterShip.Text = Rorqual Or cmbMineBoosterShip.Text = Porpoise Then
                cmbMineBoosterShipSkill.Enabled = True
            Else
                cmbMineBoosterShipSkill.Enabled = False
            End If

            If cmbMineBoosterShip.Text = Rorqual Then
                chkMineRorqDeployedMode.Enabled = True
                cmbMineIndustReconfig.Enabled = True
                lblMineIndustrialReconfig.Enabled = True
            Else
                chkMineRorqDeployedMode.Enabled = False
                cmbMineIndustReconfig.Enabled = False
                lblMineIndustrialReconfig.Enabled = False
            End If

        Else
            cmbMineBoosterShip.Enabled = False
            cmbMineMiningDirector.Enabled = False
            cmbMineMiningForeman.Enabled = False
            chkMineForemanMindlink.Enabled = False
            chkMineForemanLaserOpBoost.Enabled = False
            chkMineForemanLaserRangeBoost.Enabled = False
            lblMineBoosterShipSkill.Enabled = False
            cmbMineBoosterShipSkill.Enabled = False
            lblMineWarfareLinkSpec.Enabled = False
            cmbMineWarfareLinkSpec.Enabled = False
            chkMineRorqDeployedMode.Enabled = False
            cmbMineIndustReconfig.Enabled = False
            lblMineIndustrialReconfig.Enabled = False
        End If

    End Sub

    ' Checks all the data entered
    Private Function CheckMiningEntryData() As Boolean

        ' Check the location
        If chkMineIncludeHighSec.Checked = False And chkMineIncludeLowSec.Checked = False And chkMineIncludeNullSec.Checked = False Then
            ' Can't query any ore
            MsgBox("You must select an Ore Location", vbExclamation, Application.ProductName)
            Return False
        End If

        ' Check the Space types
        If chkMineAmarr.Checked = False And chkMineCaldari.Checked = False And chkMineGallente.Checked = False And chkMineMinmatar.Checked = False And chkMineWH.Checked = False Then
            ' Can't query any ore
            MsgBox("You must select an Ore Space", vbExclamation, Application.ProductName)
            Return False
        End If

        If chkMineWH.Checked = True And chkMineWH.Enabled = True And (chkMineC1.Checked = False And chkMineC2.Checked = False And chkMineC3.Checked = False And chkMineC4.Checked = False And chkMineC5.Checked = False And chkMineC6.Checked = False) Then
            ' Can't query any ore
            MsgBox("You must select a Wormhole Class", vbExclamation, Application.ProductName)
            Return False
        End If

        ' Check the values in the hauler calculations. They can't be greater than 30 minutes
        If (CInt(txtMineRTMin.Text) * 60) + CInt(txtMineRTSec.Text) > 1800 Then
            ' Can't query any ore
            MsgBox("Please select a smaller Round Trip Time for returning to station", vbExclamation, Application.ProductName)
            txtMineRTMin.Focus()
            Return False
        End If

        ' Check jump costs
        If chkMineIncludeJumpCosts.Checked = True Then
            If Not IsNumeric(txtMineTotalJumpFuel.Text) Or Trim(txtMineTotalJumpFuel.Text) = "" Then
                MsgBox("Invalid Jump Fuel Value", vbExclamation, Application.ProductName)
                txtMineTotalJumpFuel.Focus()
                Return False
            End If

            If Not IsNumeric(txtMineTotalJumpM3.Text) Or Trim(txtMineTotalJumpM3.Text) = "" Then
                MsgBox("Invalid Jump m3 Value", vbExclamation, Application.ProductName)
                txtMineTotalJumpM3.Focus()
                Return False
            End If

            If CDbl(txtMineTotalJumpM3.Text) <= 0 Then
                MsgBox("Jump m3 Value must be greater than zero", vbExclamation, Application.ProductName)
                txtMineTotalJumpM3.Focus()
                Return False
            End If
        End If

        ' Number of miners
        If Trim(txtMineNumberMiners.Text) = "" Or Trim(txtMineNumberMiners.Text) = "0" Then
            MsgBox("Invalid number of miners", vbExclamation, Application.ProductName)
            txtMineNumberMiners.Focus()
            Return False
        End If

        If Val(txtMineNumberMiners.Text) > 100 Then
            MsgBox("You can't select more than 100 miners", vbExclamation, Application.ProductName)
            txtMineNumberMiners.Focus()
            Return False
        End If

        ' Make sure a refine type is selected for ice and ore
        If chkMineRefinedOre.Checked = False And chkMineCompressedOre.Checked = False And chkMineUnrefinedOre.Checked = False And cmbMineOreType.Text <> "Gas" Then
            ' Can't calculate nothing
            MsgBox("You must select one ore type to calculate.", vbExclamation, Application.ProductName)
            chkMineRefinedOre.Focus()
            Return False
        End If

        ' Check the refine values
        If CDbl(txtMineRefineStanding.Text) > 10 Then
            ' Can't query any ore
            MsgBox("Please set a smaller station standing value", vbExclamation, Application.ProductName)
            txtMineRefineStanding.Focus()
            Return False
        End If

        If Not IsNumeric(cmbMineRefineStationTax.Text.Replace("%", "")) Then
            ' Can't query any ore
            MsgBox("Invalid station tax rate value", vbExclamation, Application.ProductName)
            cmbMineRefineStationTax.Focus()
            Return False
        End If

        ' Mining drones
        If Trim(txtMineMiningDroneM3.Text) = "" Then
            ' Can't query any ore
            MsgBox("Invalid mining drone m3/hour amount", vbExclamation, Application.ProductName)
            txtMineMiningDroneM3.Focus()
            Return False
        End If

        If CDbl(txtMineMiningDroneM3.Text) < 0 Then
            ' Can't query any ore
            MsgBox("Invalid mining drone m3/hour amount", vbExclamation, Application.ProductName)
            txtMineMiningDroneM3.Focus()
            Return False
        End If

        ' Check that there is a mining laser chosen
        If CStr(cmbMineMiningLaser.Text) = "" Then
            MsgBox("No mining laser selected. Check ship type and skills selected.", vbExclamation, Application.ProductName)
            cmbMineMiningLaser.Focus()
            Return False
        End If

        Return True

    End Function

    ' Returns the Mining bonus multiplier for crystals
    Private Function GetMiningCrystalBonus(ByVal OreName As String, ByRef CrystalType As String) As Double
        Dim BonusValue As Double = 1
        Dim TempCrystalType As String = ""
        Dim OreProcessingSkill As Integer

        OreProcessingSkill = GetOreProcessingSkill(OreName)

        ' See if they have T1 or T2 - and use T1 if they select T2 but can't use them
        If (OreProcessingSkill >= 3 And rbtnMineT1Crystals.Checked And rbtnMineT1Crystals.Enabled = True) _
            Or (rbtnMineT2Crystals.Checked And OreProcessingSkill = 3 And rbtnMineT2Crystals.Enabled = True) Then
            ' Use T1 bonus - TODO Look up values
            If OreName.Contains("Mercoxit") Then
                BonusValue = 1.25
            Else
                BonusValue = 1.625
            End If
            TempCrystalType = "T1"
        ElseIf OreProcessingSkill >= 4 And rbtnMineT2Crystals.Checked And rbtnMineT2Crystals.Enabled = True Then
            ' Use T2 bonus
            If OreName.Contains("Mercoxit") Then
                BonusValue = 1.375
            Else
                BonusValue = 1.75
            End If
            TempCrystalType = "T2"
        End If

        ' Add rig value for mercoxit
        If rbtnMineMercoxitRig.Checked And OreName.Contains("Mercoxit") Then
            BonusValue = BonusValue + 0.16
        End If

        If TempCrystalType <> "" Then
            CrystalType = TempCrystalType
        Else
            CrystalType = None
        End If

        Return BonusValue

    End Function

    ' Loads the cargo m3 for hauler if selected
    Private Sub RefreshHaulerM3()
        ' If the hauler is not checked and they don't have a m3 set, load the M3 of the ship selected
        If chkMineUseHauler.Checked Then
            ' Load the ore hold of the ship selected
            Select Case cmbMineShipType.Text
                Case Hulk, Skiff, Covetor, Procurer, Venture, Prospect, Endurance
                    txtMineHaulerM3.Text = FormatNumber(GetAttribute("specialOreHoldCapacity", cmbMineShipType.Text), 2)
                Case Mackinaw, Retriever
                    txtMineHaulerM3.Text = FormatNumber(GetAttribute("specialOreHoldCapacity", cmbMineShipType.Text) * (1 + (CInt(cmbMineBaseShipSkill.Text) * 0.05)), 2)
                Case Else
                    txtMineHaulerM3.Text = "0.00"
            End Select
        End If
    End Sub

    ' Calculates the total mining amount per cycle for the ship set up (not including crystals)
    Private Function CalculateMiningAmount() As Double
        Dim Mining As Double
        Dim Astrogeology As Double
        Dim BaseShipBonus As Double
        Dim Exhumers As Double
        Dim MiningForeman As Double
        Dim MiningUpgrades As Double
        Dim HighwallImplant As Double
        Dim MichiImplant As Double
        Dim RoleBonus As Double

        Dim m3YieldperCycle As Double

        If cmbMineOreType.Text = "Ore" Then

            ' Yield stacks
            If cmbMineSkill.Enabled = True Then
                Mining = CInt(cmbMineSkill.Text)
            Else
                Mining = 0
            End If

            If cmbMineAstrogeology.Enabled = True Then
                Astrogeology = CInt(cmbMineAstrogeology.Text)
            Else
                Astrogeology = 0
            End If

            If chkMineMichiImplant.Enabled = True And chkMineMichiImplant.Checked = True Then
                MichiImplant = GetAttribute(MiningAmountBonus, chkMineMichiImplant.Text)
            Else
                MichiImplant = 0
            End If

            ' Yield
            If cmbMineShipType.Enabled = True Then
                ' Can't look up the bonuses easily
                Select Case cmbMineShipType.Text
                    Case Venture, Prospect
                        ' 5% per level plus 100% role bonus
                        BaseShipBonus = 0.05 * CInt(cmbMineBaseShipSkill.Text)
                        RoleBonus = 2
                    Case Endurance
                        ' 5% per level plus 300% role bonus
                        BaseShipBonus = 0.05 * CInt(cmbMineBaseShipSkill.Text)
                        RoleBonus = 4
                    Case Else
                        BaseShipBonus = 0
                        RoleBonus = 1
                End Select
            Else
                BaseShipBonus = 0
            End If

            ' Look up each based on bonus
            If cmbMineMiningUpgrade.Enabled = True And cmbMineMiningUpgrade.Text <> None Then
                ' Replace the percent if it's in the string so we can take the 9 or 10% bonus easier
                Dim TempUpgradeText As String = cmbMineMiningUpgrade.Text.Replace("%", "")
                MiningUpgrades = CInt(TempUpgradeText.Substring(0, 2))
            Else
                MiningUpgrades = 0
            End If

            If cmbMineImplant.Text <> None Then
                'Inherent Implants 'Highwall' Mining MX-1001
                HighwallImplant = GetAttribute(MiningAmountBonus, "Inherent Implants 'Highwall' Mining " & cmbMineImplant.Text.Substring(11))
            Else
                HighwallImplant = 0
            End If

            If cmbMineExhumers.Enabled = True Then
                Select Case cmbMineShipType.Text
                    Case Prospect
                        ' 5% per level plus for the expedition frigate skill (we'll use exhumers combo)
                        Exhumers = 0.05 * CInt(cmbMineExhumers.Text)
                    Case Else
                        Exhumers = 0
                End Select
            Else
                Exhumers = 0
            End If

            ' Finally check mining foreman
            If chkMineUseFleetBooster.Checked = True Then
                If chkMineForemanMindlink.Enabled = True And chkMineForemanMindlink.Checked = True Then
                    ' They have mindlink implant, so replace with implant mining bonus
                    MiningForeman = GetAttribute(MiningAmountBonus, chkMineForemanMindlink.Text)
                ElseIf cmbMineMiningForeman.Enabled = True Then
                    ' Just use the level they have
                    MiningForeman = CInt(cmbMineMiningForeman.Text) * GetAttribute(MiningAmountBonus, lblMineMiningForeman.Text.Substring(0, Len(lblMineMiningForeman.Text) - 1))
                Else
                    MiningForeman = 1
                End If
            End If

            ' Get base yield and multiply by number of lasers
            m3YieldperCycle = GetAttribute("miningAmount", cmbMineMiningLaser.Text) * CInt(cmbMineNumLasers.Text)
            ' Add skills
            m3YieldperCycle = m3YieldperCycle * (1 + ((GetAttribute(MiningAmountBonus, "Mining") * Mining) / 100))
            m3YieldperCycle = m3YieldperCycle * (1 + ((GetAttribute(MiningAmountBonus, "Astrogeology") * Astrogeology) / 100))
            m3YieldperCycle = m3YieldperCycle * (1 + (MichiImplant / 100))
            m3YieldperCycle = m3YieldperCycle * (1 + (HighwallImplant / 100))
            m3YieldperCycle = m3YieldperCycle * ((1 + (MiningUpgrades / 100)) ^ CInt(cmbMineNumMiningUpgrades.Text)) ' Diminishing returns
            m3YieldperCycle = m3YieldperCycle * (1 + BaseShipBonus)
            m3YieldperCycle = m3YieldperCycle * (1 + Exhumers)
            m3YieldperCycle = m3YieldperCycle * (1 + (MiningForeman / 100))
            m3YieldperCycle = m3YieldperCycle * RoleBonus

            Return m3YieldperCycle

        ElseIf cmbMineOreType.Text = "Ice" Then
            ' One block per cycle
            Return (1 * CInt(cmbMineNumLasers.Text))

        ElseIf cmbMineOreType.Text = "Gas" Then
            ' Get base yield and multiply by number of lasers
            m3YieldperCycle = GetAttribute("miningAmount", cmbMineMiningLaser.Text) * CInt(cmbMineNumLasers.Text)

            ' Role(Bonus)
            ' 100% bonus to mining yield and gas cloud harvesting
            Select Case cmbMineShipType.Text
                Case Prospect, Venture
                    m3YieldperCycle = m3YieldperCycle * 2
            End Select

            Return m3YieldperCycle
        End If

        Return 0

    End Function

    ' Calculates the total burst bonus from ships and charges
    Private Function CalculateBurstBonus(BurstType As String, BoostCheckRef As CheckBox) As Double
        Dim GangBurstBonus As Double
        Dim MindLinkBonus As Double
        Dim BaseChargeBonus As Double

        If chkMineForemanMindlink.Checked = True And chkMineForemanMindlink.Enabled = True Then
            MindLinkBonus = 1.25
        Else
            MindLinkBonus = 1
        End If

        If BurstType = "Range" Then
            BaseChargeBonus = 0.3
        Else
            BaseChargeBonus = 0.15
        End If

        ' Mining Foreman Link bonus
        If cmbMineMiningDirector.Enabled = True And BoostCheckRef.Enabled Then
            If BoostCheckRef.Checked And BoostCheckRef.CheckState = CheckState.Indeterminate Then
                ' Checked T2 - T2 gives 25% bonus to charges
                GangBurstBonus = (BaseChargeBonus * 1.25) * (1 + (CInt(cmbMineMiningDirector.Text) * 0.1)) * MindLinkBonus
            ElseIf BoostCheckRef.Checked Then
                ' Checked T1
                GangBurstBonus = BaseChargeBonus * (1 + (CInt(cmbMineMiningDirector.Text) * 0.1)) * MindLinkBonus
            Else
                GangBurstBonus = 0
            End If
        Else
            GangBurstBonus = 0
        End If

        ' Ship boost to bursts
        Select Case cmbMineBoosterShip.Text
            Case Porpoise
                GangBurstBonus = GangBurstBonus * (1 + (0.02 * CInt(cmbMineBoosterShipSkill.Text)))
            Case Orca
                GangBurstBonus = GangBurstBonus * (1 + (0.03 * CInt(cmbMineBoosterShipSkill.Text)))
            Case Rorqual
                ' Rorq bonus applies only if deployed with core on
                If (chkMineRorqDeployedMode.Checked Or chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate) And chkMineRorqDeployedMode.Enabled Then
                    If chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate Then
                        GangBurstBonus = GangBurstBonus * (1 + (0.3 * CInt(cmbMineBoosterShipSkill.Text))) ' 30% max for Industrial core II
                    Else ' T1
                        GangBurstBonus = GangBurstBonus * (1 + (0.25 * CInt(cmbMineBoosterShipSkill.Text))) ' 25% max for Industrial core I
                    End If
                Else
                    GangBurstBonus = GangBurstBonus * (1 + (0.05 * CInt(cmbMineBoosterShipSkill.Text))) ' 5% without core
                End If
        End Select

        Return GangBurstBonus

    End Function

    ' Calculates the range for the miner selected and boosts applied
    Private Function CalculateMiningRange(BaseRange As Double) As Double
        Dim CalculatedRange As Double

        ' Calc range with bursts
        If chkMineUseFleetBooster.Checked Then
            CalculatedRange = BaseRange * (1 + CalculateBurstBonus("Range", chkMineForemanLaserRangeBoost))
        Else
            CalculatedRange = BaseRange
        End If

        ' Finally, if we have a hulk or covetor, then add the range bonus
        Select Case cmbMineShipType.Text
            Case Covetor, Hulk
                CalculatedRange = CalculatedRange * (1 + (0.05 * CInt(cmbMineBaseShipSkill.Text)))
        End Select

        Return CalculatedRange

    End Function

    ' Returns the cycle time of the mining laser cycle time sent
    Private Function CalculateMiningCycleTime(ByVal BaseCycleTime As Double) As Double
        Dim GangBurstBonus As Double
        Dim TempCycleTime As Double

        ' Boosters use one module and charges for boosting - Ascension
        If chkMineUseFleetBooster.Checked Then
            GangBurstBonus = CalculateBurstBonus("Cycle", chkMineForemanLaserOpBoost)
        Else
            GangBurstBonus = 0
        End If

        ' Get the adjusted time with ganglinks etc
        TempCycleTime = BaseCycleTime * (1 - GangBurstBonus)

        ' Changed with YC.118.9.1 - 9/2016
        Select Case cmbMineShipType.Text
            Case Procurer
                ' 2% reduction per level
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.02))
            Case Retriever
                ' 2% reduction per level
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.02))
            Case Covetor
                ' 2% reduction
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.02))
                TempCycleTime = TempCycleTime * (1 - 0.25) ' 25% role bonus
            Case Skiff
                ' 2% reduction per level for barges and 2% reduction for exhumers
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.02)) * (1 - (CDec(cmbMineExhumers.Text) * 0.02))
            Case Mackinaw
                ' 2% reduction per level for barges and 2% reduction for exhumers
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.02)) * (1 - (CDec(cmbMineExhumers.Text) * 0.02))
            Case Hulk
                ' 2% reduction for mining barges and 3% reduction for exhumers
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.02)) * (1 - (CDec(cmbMineExhumers.Text) * 0.03))
                TempCycleTime = TempCycleTime * (1 - 0.25) ' 25% role bonus
            Case Endurance
                ' 5% reduction for Expedition Frigate level and 5% for mining frigate level plus 50% role bonus
                If cmbMineOreType.Text = "Ice" Then
                    TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.05)) * (1 - (CDec(cmbMineExhumers.Text) * 0.05)) * (1 - 0.5)
                End If
        End Select

        If cmbMineOreType.Text = "Ice" Then
            ' For Ice, check for duration reduction bonus, implant, and upgrades
            If cmbMineGasIceHarvesting.Enabled Then
                ' Apply the ice harvesting bonus
                TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineGasIceHarvesting.Text) * 0.05))
            End If

            ' Apply the upgrades
            If cmbMineMiningUpgrade.Enabled = True And cmbMineMiningUpgrade.Text <> None Then
                ' Replace the percent if it's in the string so we can take the 9 or 10% bonus easier
                Dim TempUpgradeText As String = cmbMineMiningUpgrade.Text.Replace("%", "")
                TempCycleTime = TempCycleTime * ((1 - (CDec(CDbl(TempUpgradeText.Substring(0, 2)) / 100))) ^ CInt(cmbMineNumMiningUpgrades.Text)) ' Diminishing returns
            End If

            ' Finally include the implant value
            If cmbMineImplant.Text <> None Then
                'Inherent Implants 'Yeti' Ice Harvesting IH-1001
                TempCycleTime = TempCycleTime * (1 - (-1 * GetAttribute("iceHarvestCycleBonus", "Inherent Implants 'Yeti' Ice Harvesting " & cmbMineImplant.Text.Substring(7)) / 100))
            End If

            ' Apply the rig bonus if selected
            If rbtnMineIceRig.Checked = True Then
                ' 12% cycle reduction
                TempCycleTime = TempCycleTime * (1 - 0.12)
            End If

        ElseIf cmbMineOreType.Text = "Gas" Then
            ' Gas, look for venture ship and implant

            Select Case cmbMineShipType.Text
                Case Prospect, Venture, Endurance
                    ' 5% reduction to gas cloud harvesting duration per level
                    TempCycleTime = TempCycleTime * (1 - (CDec(cmbMineBaseShipSkill.Text) * 0.05))
            End Select

            ' Finally include the implant value
            If cmbMineImplant.Text <> None Then
                'Eifyr and Co. 'Alchemist' Gas Harvesting GH-801
                TempCycleTime = TempCycleTime * (1 - (-1 * GetAttribute("durationBonus", "Eifyr and Co. 'Alchemist' Gas Harvesting " & cmbMineImplant.Text.Substring(12)) / 100))
            End If

        End If

        Return TempCycleTime

    End Function

    ' Returns the ore processing skill level on the screen for the ore name sent
    Private Function GetOreProcessingSkill(ByVal OreName As String) As Integer
        Dim i As Integer
        Dim CurrentProcessingLabel As String

        If cmbMineOreType.Text = "Ice" Then
            OreName = "Ice Processing"
        End If

        If OreName.Contains("Ochre") Then
            OreName = "Dark Ochre"
        End If

        For i = 1 To MineProcessingCombos.Count - 1
            CurrentProcessingLabel = MineProcessingLabels(i).Text.Substring(0, InStr(MineProcessingLabels(i).Text, " ") - 1)

            ' Special processing for Dark Ochre
            If CurrentProcessingLabel = "Dark" Then
                CurrentProcessingLabel = "Dark Ochre"
            End If

            If MineProcessingCombos(i).Enabled = True And CBool(InStr(OreName, CurrentProcessingLabel)) Then
                ' Found it, return value
                Return CInt(MineProcessingCombos(i).Text)
            End If
        Next

        Return 0

    End Function

    ' Gets the amount per hour that we need to adjust the isk/hour for jump costs
    Private Function GetJumpCosts(RefinedMats As Materials, Ore As MiningOre, OreAmount As Double) As Double
        Dim SQL As String
        Dim readerORE As SQLiteDataReader
        Dim ReturnValue As Double
        Dim IskperM3 As Double ' How much each m3 in the jump ship costs based on jump fuel

        ' Ore stuff
        Dim CompressedBlocks As Double
        Dim CompressedBlockVolume As Double

        If chkMineIncludeJumpCosts.Checked = True Then
            IskperM3 = CDbl(txtMineTotalJumpFuel.Text) / CDbl(txtMineTotalJumpM3.Text)

            If rbtnMineJumpMinerals.Checked = True Then
                ' Take the volume of minerals for the hour, then multiple by the isk per m3 for total cost
                ReturnValue = IskperM3 * RefinedMats.GetTotalVolume
            Else
                ' Compressed ORE
                ' Get quanity to build 1 compressed block, divide into ore amount (allow partial). 
                ' Multiply by Compressed block m3, then multiply by iskper m3 to get cost
                SQL = "SELECT QUANTITY FROM ALL_BLUEPRINT_MATERIALS WHERE MATERIAL ='" & Ore.OreName & "' AND BLUEPRINT_NAME = 'Compressed " & Ore.OreName & " Blueprint'"
                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerORE = DBCommand.ExecuteReader

                If readerORE.Read Then
                    CompressedBlocks = OreAmount / CLng(readerORE.GetValue(0))
                Else
                    Return 0
                End If

                readerORE.Close()
                SQL = "SELECT volume FROM INVENTORY_TYPES WHERE typeName ='" & Ore.OreName & "'"
                DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
                readerORE = DBCommand.ExecuteReader

                If readerORE.Read Then
                    CompressedBlockVolume = readerORE.GetDouble(0)
                Else
                    Return 0
                End If

                ' Final value to jump this ore
                Return CompressedBlocks * CompressedBlockVolume * IskperM3

            End If

            Return ReturnValue
        Else
            Return 0
        End If

    End Function

    ' Updates the booster checks 
    Public Sub UpdateMiningBoosterObjects()

        ' Laser Op
        If chkMineForemanLaserOpBoost.Checked And chkMineForemanLaserOpBoost.CheckState = CheckState.Indeterminate Then ' Show T2
            chkMineForemanLaserOpBoost.Text = "Mining Foreman Link T2 - Laser Optimization Charge"
            chkMineForemanLaserOpBoost.ForeColor = Color.DarkOrange

            If File.Exists(Path.Combine(UserImagePath, "43551_32.png")) Then
                pictMineLaserOptmize.Image = Image.FromFile(Path.Combine(UserImagePath, "43551_32.png"))
            Else
                pictMineLaserOptmize.Image = Nothing
            End If

            pictMineLaserOptmize.Update()

        Else
            chkMineForemanLaserOpBoost.Text = "Mining Foreman Link - Laser Optimization Charge"
            chkMineForemanLaserOpBoost.ForeColor = Color.Black

            If File.Exists(Path.Combine(UserImagePath, "42528_32.png")) Then
                pictMineLaserOptmize.Image = Image.FromFile(Path.Combine(UserImagePath, "42528_32.png"))
            Else
                pictMineLaserOptmize.Image = Nothing
            End If

            pictMineLaserOptmize.Update()
        End If

        ' Range 
        If chkMineForemanLaserRangeBoost.Checked And chkMineForemanLaserRangeBoost.CheckState = CheckState.Indeterminate Then ' Show T2
            chkMineForemanLaserRangeBoost.Text = "Mining Foreman Link T2 - Mining Laser Field Enhancement Charge"
            chkMineForemanLaserRangeBoost.ForeColor = Color.DarkOrange

            If File.Exists(Path.Combine(UserImagePath, "43551_32.png")) Then
                pictMineRangeLink.Image = Image.FromFile(Path.Combine(UserImagePath, "43551_32.png"))
            Else
                pictMineRangeLink.Image = Nothing
            End If
        Else
            chkMineForemanLaserRangeBoost.Text = "Mining Foreman Link - Mining Laser Field Enhancement Charge"
            chkMineForemanLaserRangeBoost.ForeColor = Color.Black

            If File.Exists(Path.Combine(UserImagePath, "42528_32.png")) Then
                pictMineRangeLink.Image = Image.FromFile(Path.Combine(UserImagePath, "42528_32.png"))
            Else
                pictMineRangeLink.Image = Nothing
            End If

            pictMineRangeLink.Update()
        End If

    End Sub

    ' Processes the industrial core checks
    Private Sub UpdateIndustrialCoreCheck()

        If chkMineRorqDeployedMode.Checked And chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate Then ' Show T2
            chkMineRorqDeployedMode.Text = "Industrial Core II Active"
            chkMineRorqDeployedMode.ForeColor = Color.DarkOrange
        ElseIf chkMineRorqDeployedMode.Checked And chkMineRorqDeployedMode.CheckState = CheckState.Checked Then ' Show T1 
            chkMineRorqDeployedMode.Text = "Industrial Core I Active"
            chkMineRorqDeployedMode.ForeColor = Color.Black
        Else
            ' Inactive
            chkMineRorqDeployedMode.Text = "Industrial Core Inctive"
            chkMineRorqDeployedMode.ForeColor = Color.Black
        End If

        If chkMineRorqDeployedMode.Checked = True Then
            lblMineIndustrialReconfig.Enabled = True
            cmbMineIndustReconfig.Enabled = True
        Else
            lblMineIndustrialReconfig.Enabled = False
            cmbMineIndustReconfig.Enabled = False
        End If

    End Sub

    ' Calculates the cost for one hour of heavy water for boosting with a Rorqual - TODO new rorq changes?
    Private Function CalculateRorqDeployedCost(IndustrialReconfigSkill As Integer, CapIndustrialShipSkill As Integer) As Double
        Dim SQL As String
        Dim readerHW As SQLiteDataReader

        Const T1CoreHWUsage As Double = 1000 ' 1000 base use for T1 Core
        Const T2CoreHWUsage As Double = 1500 ' 1500 base use for T1 Core

        Dim HWUsage As Double = 0

        If chkMineRorqDeployedMode.Enabled = True Then
            If chkMineRorqDeployedMode.Checked And chkMineRorqDeployedMode.CheckState = CheckState.Indeterminate Then
                ' Checked T2
                HWUsage = T2CoreHWUsage
            ElseIf chkMineRorqDeployedMode.Checked Then
                ' Checked T1
                HWUsage = T1CoreHWUsage
            Else
                HWUsage = 0
            End If
        Else
            HWUsage = 0
        End If

        ' Users can set Industrial Reconfig to 0 - this is 0 cost or not calculating cost
        If IndustrialReconfigSkill = 0 Then
            Return 0
        End If

        ' Skill at the operation of industrial core modules.  
        ' 50-unit reduction in heavy water consumption amount for module activation per skill level.
        HWUsage = HWUsage - (IndustrialReconfigSkill * 50)

        ' Capital Industrial Ships skill bonuses:
        ' -5% reduction in fuel consumption for industrial cores per level
        HWUsage = HWUsage - (HWUsage * 0.05 * CapIndustrialShipSkill)

        ' Look up the cost for Heavy Water
        SQL = "SELECT PRICE FROM ITEM_PRICES WHERE ITEM_NAME = 'Heavy Water'"
        DBCommand = New SQLiteCommand(SQL, EVEDB.DBREf)
        readerHW = DBCommand.ExecuteReader

        If readerHW.Read Then
            ' Return cost for one hour (cycle is 5 minutes)
            Return HWUsage * readerHW.GetDouble(0) * 12
        Else
            Return 0
        End If

    End Function

    ' The Ore structure to display in our grid for mining
    Public Structure MiningOre
        Dim OreID As Long
        Dim OreName As String
        Dim OreUnitPrice As Double
        Dim RefineYield As Double
        Dim CrystalType As String
        Dim OreVolume As Double
        Dim IPH As Double
        Dim UnitsPerHour As Double
        Dim OreUnitsPerCycle As Double
        Dim UnitsToRefine As Integer
        Dim RefineType As String
    End Structure

    ' For sorting a list of Mining Ore
    Public Class MiningOreIPHComparer

        Implements System.Collections.Generic.IComparer(Of MiningOre)

        Public Function Compare(ByVal p1 As MiningOre, ByVal p2 As MiningOre) As Integer Implements IComparer(Of MiningOre).Compare
            ' swap p2 and p1 to do decending sort
            Return p2.IPH.CompareTo(p1.IPH)
        End Function

    End Class

#End Region

End Class