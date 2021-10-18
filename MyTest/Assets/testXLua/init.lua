XLuaMgr = {}

require "default.CommClass.CommClass"
require "module.ModuleMgr"

require "GameLogicInterface"
require "Soul"

function XLuaMgr:OnStart()
    print("xlua OnStart")

    local bCloseWD = true
    local bEnableAsyncRes = false
    Game:Initialize(bCloseWD, bEnableAsyncRes)
end

function XLuaMgr:OnProcess()
end

function XLuaMgr:OnPostProcess()
    Game:ProcessGame(Time.deltaTime * 1000)
end

function XLuaMgr:OnGUI()
    if Event.current.type == EventType.MouseDown then
        XLuaMgr:ProcessMouseDown()
    elseif Event.current.type == EventType.MouseUp then
        XLuaMgr:ProcessMouseUp()
    elseif Event.current.type == EventType.ScrollWheel then
        XLuaMgr:ProcessScrollWheel()
    elseif Event.current.type == EventType.KeyDown then
        XLuaMgr:ProcessKeyDown();
    elseif Event.current.type == EventType.KeyUp then
        XLuaMgr:ProcessKeyUp();
    else
        XLuaMgr:ProcessMouseMove()
    end
end

function XLuaMgr:OnStop()
    print("xlua OnStop")

    Game:Destory()
end

function XLuaMgr:ProcessMouseDown()
    local vPos = XLuaMgr:GetMousePosition()
    if Event.current.button == 0 then
        if Event.current.clickCount == 2 then
            Game:OnLButtonDblClk(vPos.x, vPos.y)
        else
            Game:OnLButtonDown(vPos.x, vPos.y, 0)
        end
    elseif Event.current.button == 1 then
        Game:OnRButtonDown(vPos.x, vPos.y)
    end
end

function XLuaMgr:ProcessMouseUp()
    local vPos = XLuaMgr:GetMousePosition()
    if Event.current.button == 0 then
        Game:OnLButtonUp(vPos.x, vPos.y, 0)
    elseif Event.current.button == 1 then
        Game:OnRButtonUp(vPos.x, vPos.y)
    end
end

function XLuaMgr:ProcessScrollWheel()
    local zDelta = Event.current.delta.y
    Game:OnMouseWheel(zDelta)
end

function XLuaMgr:ProcessKeyDown()
    self:OnKeyDown();
end

function XLuaMgr:ProcessKeyUp()
    self:OnKeyUp();
end

function XLuaMgr:ProcessMouseMove()
    local vPos = XLuaMgr:GetMousePosition()
    if nil ~= XLuaMgr.vLastPos then
        local vDelta = vPos - XLuaMgr.vLastPos
        if vDelta ~= Vector2Int.zero then
            Game:OnMouseMove(vPos.x, vPos.y, 0)
        end
    else
        Game:OnMouseMove(vPos.x, vPos.y, 0)
    end
    XLuaMgr.vLastPos = vPos
end

function XLuaMgr:GetMousePosition()
    local x = Event.current.mousePosition.x
    local y = Event.current.mousePosition.y
    local vPos = Vector2Int()
    vPos.x = x
    vPos.y = y
    return vPos
end

function XLuaMgr:OnKeyDown()
    if XLuaMgr.modules.fguiEdit ~= nil and XLuaMgr.modules.fguiEdit:IsEditOpen() then return end
    local uiKey = 0
    if Input.GetKeyDown(KeyCode.W) then
        uiKey = 0x57
    elseif Input.GetKeyDown(KeyCode.A) then
        uiKey = 0x41
    elseif  Input.GetKeyDown(KeyCode.S) then
        uiKey = 0x53
    elseif Input.GetKeyDown(KeyCode.D) then
        uiKey = 0x44
    end

    if uiKey ~= 0 then
        Game:OnKeyDown(uiKey)
    end
end

function XLuaMgr:OnKeyUp()
    if XLuaMgr.modules.fguiEdit ~= nil and XLuaMgr.modules.fguiEdit:IsEditOpen() then return end
    local uiKey = 0
    if Input.GetKeyUp(KeyCode.W) then
        uiKey = 0x57
    elseif Input.GetKeyUp(KeyCode.A) then
        uiKey = 0x41
    elseif Input.GetKeyUp(KeyCode.S) then
        uiKey = 0x53
    elseif Input.GetKeyUp(KeyCode.D) then
        uiKey = 0x44
    elseif Input.GetKeyUp(KeyCode.Return) then
        uiKey = 0x0D
    elseif Input.GetKeyUp(KeyCode.Print) then
        uiKey = 0x0C
    elseif Input.GetKeyUp(KeyCode.Escape) then
        uiKey = 0x1B
    end

    if uiKey ~= 0 then
        Game:OnKeyUp(uiKey)
    end
end

return XLuaMgr
