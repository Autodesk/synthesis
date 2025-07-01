import { expect, test } from "vitest"
import FieldMiraEditor from "../mirabuf/FieldMiraEditor"

function mockParts(): any {
  return { userData: { data: {} } }
}

test("writes and reads devtool data", () => {
  const parts = mockParts()
  const editor = new FieldMiraEditor(parts)

  const key = "devtool:scoring_zones"
  const payload = [{ id: "zone-A", pose: { position:{x:0,y:0,z:0}, rotation:{x:0,y:0,z:0,w:1} }, size:{x:1,y:1,z:1} }]

  editor.setUserData(key, payload)
  expect(editor.getUserData(key)).toEqual(payload)
  expect(editor.getAllDevtoolKeys()).toContain(key)

  editor.removeUserData(key)
  expect(editor.getUserData(key)).toBeUndefined()
})