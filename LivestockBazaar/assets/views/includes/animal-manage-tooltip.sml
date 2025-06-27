<frame border={@Mods/StardewUI/Sprites/ControlBorder} padding="12">
  <lane orientation="vertical" layout="content content">
    <label text={:DisplayName} font="dialogue" padding="4" />
    <label text={:DisplayType} font="small" padding="4" />
    <lane orientation="horizontal" layout="content content" padding="4">
      <panel *repeat={:Hearts}>
        <image *if={:this} sprite={@mushymato.LivestockBazaar/sprites/cursors:heartFull} layout="32px 24px"/>
        <image *!if={:this} sprite={@mushymato.LivestockBazaar/sprites/cursors:heartEmpty} layout="32px 24px"/>
      </panel>
    </lane>
    <image *if={Held} sprite={:Sprite} layout={:SpriteLayout}/>
  </lane>
</frame>